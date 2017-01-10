using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorCommand
{
    private Vector3 _targetWorldPos = Vector3.zero;
    private Vector3 _direction = Vector3.zero;
    private System.Action<int> _callBack = null;
    private int _targetFloorIndex = 0;
    private List<Person> boardedPersonList = new List<Person>(); // 탑승인원

    public Vector3 TargetWorldPos { get { return this._targetWorldPos; } }
    public Vector3 Direction { get { return this._direction; } }
    public System.Action<int> CallBack { get { return this._callBack; } }
    public int TargetFloorIndex { get { return _targetFloorIndex; } }

    public ElevatorCommand(int targetFloorIndex, Vector3 startWorldPos, Vector3 targetWorldPos, System.Action<int> callBack)
    {
        this._targetFloorIndex = targetFloorIndex;
        this._targetWorldPos = targetWorldPos;
        this._direction = this.GetDirectionToGo(startWorldPos, targetWorldPos);
        this._callBack = callBack;
    }

    private Vector3 GetDirectionToGo(Vector3 selfWorldPos, Vector3 targetWorldPos)
    {
        Vector3 direction = Vector3.zero;
        if (selfWorldPos.y < targetWorldPos.y)
            direction = Vector3.up;
        else
            direction = Vector3.down;
        return direction;
    }
}

public class Elevator : MonoBehaviour
{
    public Transform passengerLine_ = null;

    public enum eSchedule {STANDBY = 0, UP = 1, DOWN = 2}; // 다음 예정 행동
    public eSchedule _schedule = eSchedule.STANDBY;
    private List<ElevatorObserver> _observerList = new List<ElevatorObserver>();
    private List<Person> _passengerList = new List<Person>();
    private List<int> _targetFloorList = new List<int>();
    
    private int _elevatorIndex;
    private int operationCount = 0;
    private bool _isArrivalAction = false;
    private int _currentFloorIndex = 0; // 정지하지 않아도 움직이는 위치에 의해서 계속 업데이트됨 

    public void Initialize(int index)
    {
        this._elevatorIndex = index;
        //Debug.Log(string.Format("엘레베이터 생성 인덱스 할당 : {0}호기", index));
    }
    #region observer
    public void RegisterObserver(ElevatorObserver o)
    {
        _observerList.Add(o);
    }

    public void UnregisterObserver(ElevatorObserver o)
    {
        _observerList.Remove(o);
    }

    public void UpdateElevatorInfo()
    {
        for (int i = 0; i < _observerList.Count; i++)
            _observerList[i].UpdateIndigator(0);
    }
    #endregion

    public void Start()
    {
    }

    public void Update()
    {
        this._currentFloorIndex = GVallyPlaza.Instance.GetFlootValueFromHeight(this.transform.localPosition.y);
    }

    public void OrderedToWork(int targetFloorIndex)
    {
        bool isComplete = this.AddTargetFloorList(targetFloorIndex);
        if (isComplete) //@breif 중복 호출을 피하기 위한 조치
        {
            StartCoroutine(Work());
            Debug.Log(string.Format("{0}층으로 운행을 명 받았습니다.", targetFloorIndex));
        }
        else
        {
            Debug.Log(string.Format("{0}층은 이미 명령 받은 층입니다.", targetFloorIndex));
        }
    }

    private bool AddTargetFloorList(int floorIndex)
    {
        bool result = false;
        if (_targetFloorList.Contains(floorIndex))
            return result;

        switch (this._schedule)
        {
            case eSchedule.UP:
                if (this._currentFloorIndex >= floorIndex)
                {
                    Debug.Log("이미 지니가서 오더를 넣을수 없습니다.");
                    return result;
                }
                break;
            case eSchedule.DOWN:                
                if(this._currentFloorIndex <= floorIndex)
                {
                    Debug.Log("이미 지니가서 오더를 넣을수 없습니다.");
                    return result;
                }                
                break;                
        }

        result = true;

        _targetFloorList.Add(floorIndex);
        _targetFloorList.Sort();

        if(_schedule == eSchedule.DOWN)
        {
            _targetFloorList.Reverse();
        }

        return result;
    }

    private void RemoveTargetFloorList(int floorIndex)
    {
        if (!_targetFloorList.Contains(floorIndex))
            return;

        _targetFloorList.Remove(floorIndex);
    }

    private IEnumerator Work()
    {
        yield return StartCoroutine(HoldOnArrivalAction());
        this.MoveToDestination(this.MovingEnd);
    }

    private void MoveToDestination(System.Action<int> callBack)
    {
        StopCoroutine("Moving");
        int targetFloorIndex = _targetFloorList[0];
        Vector3 targetPos = GVallyPlaza.Instance.GetFloorComponent(targetFloorIndex).GetGateWorldPosition(this._elevatorIndex);
        ElevatorCommand command = new ElevatorCommand(targetFloorIndex, this.transform.position, targetPos, callBack);
        Debug.Log(string.Format("{0}호기, 목표:{1}층, 운행카운트:{2}", this._elevatorIndex, targetFloorIndex, operationCount));
        // @TODO: 추후에 효과 적으로 수정한다 여기단에서 끊지 않으면 진행을 할수없어서 임시방편...
        if (command.Direction == Vector3.up)
        {
            _schedule = eSchedule.UP;
        }
        else
        {
            _schedule = eSchedule.DOWN;
        }        
        StartCoroutine("Moving", command);
    }

    private IEnumerator Moving(ElevatorCommand command)
    {
        float dist = 0.0f;

        do
        {
            dist = Vector3.Distance(this.transform.position, command.TargetWorldPos);
            this.transform.Translate(command.Direction * Time.deltaTime * GVallyPlaza.ELEVATOR_SPEED);
            yield return null;

            //if (command.TargetFloorIndex == 15)
            //    OrderedToWork(1);
        } while (dist >= 0.1f);

        Floor arrivalFloorComponent = GVallyPlaza.Instance.GetFloorComponent(command.TargetFloorIndex);
        IEnumerator arrivalEvent = ArrivalAction(arrivalFloorComponent);
        yield return StartCoroutine(arrivalEvent);

        operationCount++;
        this.transform.position = command.TargetWorldPos;
        if (command.CallBack != null)
            command.CallBack(command.TargetFloorIndex);
    }

    private void MovingEnd(int currentFloor)
    {
        this.RemoveTargetFloorList(currentFloor);

        if(_targetFloorList.Count > 0)
        {
            MoveToDestination(this.MovingEnd);
        }
        else
        {
            this._schedule = eSchedule.STANDBY;
        }
    }

    private IEnumerator HoldOnArrivalAction()
    {
        while (_isArrivalAction)
        {
            yield return null;
        }
    }

    private IEnumerator ArrivalAction(Floor currentFloorComponent)
    {
        _isArrivalAction = true;
        yield return StartCoroutine("GateOpen"); // 1. 문이 열리고

        List<Person> waitingList = this.GetFloorWaitingList(currentFloorComponent, this._schedule);
        List<Person> exitList = MakeExitPersonList(this._passengerList, currentFloorComponent.FloorIndex);
        
        if (exitList.Count > 0)
        {
            IEnumerator exitEvent = ExitPerson(this._passengerList, exitList, currentFloorComponent);
            yield return StartCoroutine(exitEvent); // 사람들이 내리고
        }

        List<Person> enterList = this.MakeEnterPersonList(this._passengerList, waitingList, 20);
        
        if (enterList.Count > 0)
        {
            IEnumerator enterEvent = EnterPerson(this._passengerList, enterList, currentFloorComponent, 20);
            yield return StartCoroutine(enterEvent); // 사람들이 타고
        }

        if (exitList.Count <=0 || enterList.Count <= 0)
        {
            yield return WaitForTheGateToOpen();
        }
        
        yield return StartCoroutine("GateClose");
        _isArrivalAction = false;
    }

    private IEnumerator GateOpen()
    {
        //TODO: 문이 열릴수 있는 조건 1. 포지션이 정확한가 2.엘레베이터는 확실히 정지한 상태인가?
        Debug.Log(string.Format("{0}호기 : 문이 열립니다.", _elevatorIndex));               
        yield return GVallyPlaza.GateOperationSec; // 시간으로 계산하기보다는 실제 닫히걸 계산 해줘야 한다, 닫히다가 열리는경우도 있기에 일단 시간으로
    }

    private IEnumerator WaitForTheGateToOpen()
    {
        Debug.Log(string.Format("{0}호기 : 문이 열린후 대기합니다", _elevatorIndex));
        yield return GVallyPlaza.WaitForTheDoorToOpenSec;
    }

    private IEnumerator GateClose()
    {
        //TODO: 문이 닫힐수 있는 조건 1.초과탑승이지 않는가? 2.게이트센서에 걸리지 않는가?
        Debug.Log(string.Format("{0}호기 : 문이 닫힙니다.", _elevatorIndex));
        yield return GVallyPlaza.GateOperationSec; // 시간으로 계산하기보다는 실제 닫히걸 계산 해줘야 한다, 닫히다가 열리는경우도 있기에 일단 시간으로
    }

    /* @brief 실제 층에서 엘레베이터에 탑승하는 기능 구현 
     */
    private IEnumerator EnterPerson(List<Person> passengerList, List<Person> enterList, Floor currentFloorComponent, int limitCount)
    {
        int acceptableCount = limitCount - passengerList.Count;

        for (int i = 0; i < acceptableCount &&  i < enterList.Count; i++)
        {
            passengerList.Add(enterList[i]);
            //currentfloorcomponent.exitfloorperson(enterlist[i]);
            enterList[i].ExitFloor(currentFloorComponent);
            Debug.Log(string.Format("{0}이 {1}층에서 엘베에 탑승했습니다.", enterList[i].Name, currentFloorComponent.FloorIndex));
            UpdatePassengerLine();
            yield return new WaitForSeconds(0.15f);
        }         
    }

    /* @brief 실제 탑승인원 목록에서 내릴 사람들을 빼주고 사람들이 내리는 시간을 지연시켜줌
     */
    private IEnumerator ExitPerson(List<Person>passenegerList, List<Person> exitList, Floor currentFloorComponent)
    {
        for (int i = 0; i < exitList.Count; i++)
        {
            if(passenegerList.Contains(exitList[i]))
            {
                passenegerList.Remove(exitList[i]);
                //currentFloorComponent.EnterFloorPerson(exitList[i]);
                exitList[i].EnterFloor(currentFloorComponent);
                Debug.Log(string.Format("{0}이 {1}층에서 엘베에서 하차했습니다.", exitList[i].Name, currentFloorComponent.FloorIndex));
                UpdatePassengerLine();
                yield return new WaitForSeconds(0.15f);
            }            
        }
    }

    /* @brief 현재층에서 엘레베이터에 탑승할 인원 리스트를 반환
    * @Param (엘레베이터에 탑승해있는 인원리스트, 층에 탑승을 희망하는 인원, 엘레베이터의 최대 탑승인원)
    */
    private List<Person> MakeEnterPersonList(List<Person> passengerList, List<Person> enterList, int limitCount)
    {
        List<Person> inList = new List<Person>();
        int acceptableCount = limitCount - passengerList.Count;

        for (int i = 0; i < enterList.Count && i < acceptableCount; i++)
        {
            inList.Add(enterList[i]);
        }
        return inList;
    }

    /* @brief 엘레베이터에 탑승한 인원 중 현재 층에서 내릴 인원 리스트를 반환
     */
    private List<Person> MakeExitPersonList(List<Person> passengerList, int outFloorValue)
    {
        List<Person> outList = new List<Person>();

        for(int i=0; i<passengerList.Count; i++)
        {
            if (outFloorValue == passengerList[i].TargetFloor)
                outList.Add(passengerList[i]);
        }
        return outList;
    }

    // @Breif 현재층에서 엘레베이터의 스케쥴에 따라 해당층에서 대기중인 전체 목록을 가져온다
    private List<Person> GetFloorWaitingList(Floor floor, eSchedule schedlue)
    {
        List<Person> waitingList = null;
        if(schedlue == eSchedule.UP)
        {
            waitingList = floor.WaitingUpList;
        }
        else if(schedlue == eSchedule.DOWN)
        {
            waitingList = floor.WaitingDownList;
        }
        return waitingList;
    }

    private bool GoodFitDocking()
    {
        return true;
    }

    private void UpdatePassengerLine()
    {
        int baseValue = _elevatorIndex < 4 ? -1 : 1;
        for(int i=0; i<_passengerList.Count; i++)
        {
            _passengerList[i].transform.parent = passengerLine_;
            _passengerList[i].transform.localPosition = new Vector3(baseValue* (i+1), 0.0f, 0.0f);
        }
    }
}
