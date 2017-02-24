using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorCommand
{
    public System.Action<int> CallBack { get { return this._callBack; } }
    public Transform TargetTranform { get { return this._targetTranform; } }
    public Vector3 Direction { get { return this._direction; } }
    public int TargetFloorIndex { get { return _targetFloorIndex; } }

    private System.Action<int> _callBack = null;
    private Transform _targetTranform = null;
    private Vector3 _direction = Vector3.zero;
    private List<Person> boardedPersonList = new List<Person>(); // 탑승인원
    private int _targetFloorIndex = 0;

    public ElevatorCommand(int targetFloorIndex, Transform thisTransform, Transform targetTransform, System.Action<int> callBack)
    {
        this._targetFloorIndex = targetFloorIndex;
        this._targetTranform = targetTransform;
        this._direction = this.GetDirectionToGo(thisTransform, targetTransform);
        this._callBack = callBack;
    }

    private Vector3 GetDirectionToGo(Transform thisTransform, Transform targetTransform)
    {
        Vector3 direction = Vector3.zero;
        if (thisTransform.position.y < targetTransform.position.y)
            direction = Vector3.up;
        else if (thisTransform.position.y > targetTransform.position.y)
            direction = Vector3.down;
        else if (thisTransform.position.y == targetTransform.position.y)
            direction = Vector3.zero;
        return direction;
    }
}

public class Elevator : MonoBehaviour
{
    public enum eDirection { UP = 0, DOWN = 1, STANDBY = 2, TURNING = 3 }; //방향 

    public eDirection Schedule { get { return this._schedule; } }
    public eDirection Direction { get { return this._direction; } }
    public int PassengerClearanceCount { get { return (LIMITED_PASSENGER_COUNT - this._passengerList.Count); } }
    public int PassengerCount { get { return this._passengerList.Count; } }
    public int CurrentFloorIndex { get { return this._currentFloorIndex; } }
    public int ElevatorIndex { get { return this._elevatorIndex; } }

    public Transform passengerLine_ = null;

    private eDirection _schedule = eDirection.STANDBY;
    private eDirection _direction = eDirection.STANDBY;

    private List<ElevatorObserver> _observerList = new List<ElevatorObserver>();
    private List<Person> _passengerList = new List<Person>();
    private List<int> _targetFloorList = new List<int>();

    private int _elevatorIndex = 0;
    private int _operationCount = 0;
    private int _currentFloorIndex = 0; // 정지하지 않아도 움직이는 위치에 의해서 계속 업데이트됨
    private int _turningFloorIndex = 0; // 터닝포인트 층의 값

    private bool _isArrivalAction = false;
    private bool _isTurning = false; // ex) 내려가기 위해 올라간다던지 그런 상황인가?
    
    private const int LIMITED_PASSENGER_COUNT = 20;

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

    public bool IsCanStopFloor(eDirection orderWant, int orderFloorIndex)
    {
        bool result = false;

        if (_isTurning)
        {
            if (this._schedule != eDirection.STANDBY && orderWant != this._schedule) // 엘베가 움직이는 방향과 오더의 방향이 다르면 리턴
            {
                return result;
            }

            switch (_schedule)
            {
                case eDirection.UP:
                    {
                        result = (_turningFloorIndex >= this._currentFloorIndex);
                    }
                    break;
                case eDirection.DOWN:
                    {
                        result = (_turningFloorIndex <= this._currentFloorIndex);
                    }
                    break;
                case eDirection.STANDBY:
                    {
                        result = true;
                    }
                    break;
            }
        }
        else
        {
            if (this._schedule != eDirection.STANDBY && orderWant != this._direction) // 엘베가 움직이는 방향과 오더의 방향이 다르면 리턴
            {
                return result;
            }

            switch (_direction)
            {
                case eDirection.UP:
                    {
                        result = (orderFloorIndex >= this._currentFloorIndex);
                    }
                    break;
                case eDirection.DOWN:
                    {
                        result = (orderFloorIndex <= this._currentFloorIndex);
                    }
                    break;
                case eDirection.STANDBY:
                    {
                        result = true;
                    }
                    break;
            }
        }

        if (!result)
        {
            return result;
        }

        result = this.PassengerClearanceCount > 0;

        return result;
    }

    // @Breif 층에서 호출시 지밸리플라자를 통해 호출됨(외부 호출) 
    public void OrderedToWork(bool isTurning, int targetFloorIndex)
    {
        this._isTurning = isTurning;
        if(_isTurning)
        {
            _turningFloorIndex = targetFloorIndex;
        }
        this.OrderedToWork(targetFloorIndex);
    }

    // @Brief 엘레베이터에 탑승해서 목적층 버튼을 누른경우 호출됨
    private void OrderedToWork(int targetFloorIndex) 
    {
        bool isComplete = this.AddTargetFloorList(targetFloorIndex);
        if (isComplete) //@breif 중복 호출을 피하기 위한 조치
        {
            if (_isTurning)
            {
                Debug.Log(string.Format("{0}호기 엘베는 터닝 하기 위한 오더를 받았습니다.", targetFloorIndex));
            }
            else
            {
                Debug.Log(string.Format("{0}호기 엘베는 {1}층으로 운행을 명 받았습니다.", this._elevatorIndex, targetFloorIndex));
            }
            StartCoroutine("Work");
        }
        else
        {
            Debug.Log(string.Format("{0}호기 엘베는 {0}층을 이미 명령 받았었습니다.", targetFloorIndex));
        }
    }

    private bool CheckIsTurning(eDirection schdule, int currentFloorIndex, int targetFloorIndex)
    {
        bool result = false;
        return result;
    }

    private bool AddTargetFloorList(int floorIndex)
    {
        bool result = false;
        if (_targetFloorList.Contains(floorIndex))
            return result;

        if (_isTurning) // 현재 운행중인 엘레베이터만 해당이 된다 (운행중이면서 이미 터닝을 위해 움직이는 경우)
        {
            switch (_schedule)
            {
                case eDirection.UP:
                    if (this._turningFloorIndex >= floorIndex)
                    {
                        Debug.Log("터닝 층 보다 높은층이라 오더를 넣을수 없습니다.");
                        return result;
                    }
                    break;
                case eDirection.DOWN:
                    if (this._turningFloorIndex <= floorIndex)
                    {
                        Debug.Log("터닝 층 보다 낮은층이라 오더를 넣을수 없습니다.");
                        return result;
                    }
                    break;
            }
        }
        else
        {
            switch (_schedule)
            {
                case eDirection.UP:
                    if (this._currentFloorIndex >= floorIndex)
                    {
                        Debug.Log("이미 지니가서 오더를 넣을수 없습니다.");
                        return result;
                    }
                    break;
                case eDirection.DOWN:
                    if (this._currentFloorIndex <= floorIndex)
                    {
                        Debug.Log("이미 지니가서 오더를 넣을수 없습니다.");
                        return result;
                    }
                    break;
            }

        }

        result = true;
        _targetFloorList.Add(floorIndex);
        return result;
    }

    private void RemoveTargetFloorList(int floorIndex)
    {
        if (!_targetFloorList.Contains(floorIndex))
            return;

        _targetFloorList.Remove(floorIndex);
    }

    private void TargetFLoorListSort(eDirection schedule)
    {
        _targetFloorList.Sort();
        if(schedule == eDirection.DOWN)
        {
            _targetFloorList.Reverse();
        }
    }

    private IEnumerator Work()
    {
        yield return StartCoroutine(HoldOnArrivalAction());
        TargetFLoorListSort(this._schedule);
        this.MoveToDestination(this.MovingEnd);
    }

    private void MoveToDestination(System.Action<int> callBack)
    {
        StopCoroutine("Moving");
        int targetFloorIndex = _targetFloorList[0];
        Transform targetTransform = GVallyPlaza.Instance.GetFloorComponent(targetFloorIndex).GetGateTranform(this._elevatorIndex);
        ElevatorCommand command = new ElevatorCommand(targetFloorIndex, this.transform, targetTransform, callBack);
        Debug.Log(string.Format("{0}호기, 목표:{1}층, 운행카운트:{2}", this._elevatorIndex, targetFloorIndex, _operationCount));
        // @TODO: 추후에 효과 적으로 수정한다 여기단에서 끊지 않으면 진행을 할수없어서 임시방편...

        if (command.Direction == Vector3.up)
        {
            _schedule = _isTurning ? eDirection.DOWN : eDirection.UP;
            _direction = eDirection.UP;
        }
        else if(command.Direction == Vector3.down)
        {
            _schedule = _isTurning ? eDirection.UP : eDirection.DOWN;
            _direction = eDirection.DOWN;
        }
        else if (command.Direction == Vector3.zero)
        {
            _schedule = eDirection.STANDBY;
            _direction = eDirection.STANDBY;
        }
                 
        StartCoroutine("Moving", command);
    }

    private IEnumerator Moving(ElevatorCommand command)
    {
        do
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, command.TargetTranform.position, GVallyPlaza.ELEVATOR_SPEED * Time.deltaTime);
            yield return null;
        } while (this.transform.position != command.TargetTranform.position);

        Floor arrivalFloorComponent = GVallyPlaza.Instance.GetFloorComponent(command.TargetFloorIndex);
        IEnumerator arrivalEvent = ArrivalAction(arrivalFloorComponent);
        yield return StartCoroutine(arrivalEvent);

        _operationCount++;
        if (command.CallBack != null)
            command.CallBack(command.TargetFloorIndex);
    }

    private void MovingEnd(int currentFloor)
    {
        this.RemoveTargetFloorList(currentFloor);

        if(_targetFloorList.Count > 0)
        {
            //MoveToDestination(this.MovingEnd);

            StartCoroutine("Work");
        }
        else
        {
            this._schedule = eDirection.STANDBY;
            GVallyPlaza.Instance.ElevatorStranBy(this);
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
        if(_isTurning)
        {
            _isTurning = false;
            _turningFloorIndex = 0;
            Debug.Log(string.Format("{0}호기 엘베가 {1}층에서 터닝하기 위해 도착했습니다.", _elevatorIndex, currentFloorComponent.FloorIndex));
        }
        else
        {
            Debug.Log(string.Format("{0}호기 엘베가 {1}층에 도착했습니다.", this._elevatorIndex, currentFloorComponent.FloorIndex));
        }

        yield return StartCoroutine("GateOpen"); // 1. 문이 열리고

        List<Person> waitingList = this.GetFloorWaitingList(currentFloorComponent, this._schedule);
        List<Person> exitList = MakeExitPersonList(this._passengerList, currentFloorComponent.FloorIndex);
        
        if (exitList.Count > 0)
        {
            IEnumerator exitEvent = ExitPerson(this._passengerList, exitList, currentFloorComponent);
            yield return StartCoroutine(exitEvent); // 사람들이 내리고
        }

        List<Person> enterList = this.MakeEnterPersonList(this._passengerList, waitingList, LIMITED_PASSENGER_COUNT);
        
        if (enterList.Count > 0)
        {
            IEnumerator enterEvent = EnterPerson(this._passengerList, enterList, currentFloorComponent, LIMITED_PASSENGER_COUNT);
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
            Debug.Log(string.Format("{0}이 {1}층에서 엘베에 탑승했고 {2}층 버튼을 눌렀습니다.", enterList[i].Name, currentFloorComponent.FloorIndex, enterList[i].TargetFloor));
            this.OrderedToWork(enterList[i].TargetFloor); // 사람이 탑승후 목적층을 눌렀다
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
    private List<Person> GetFloorWaitingList(Floor floor, eDirection schedlue)
    {
        List<Person> waitingList = null;
        if(schedlue == eDirection.UP)
        {
            waitingList = floor.WaitingUpList;
        }
        else if(schedlue == eDirection.DOWN)
        {
            waitingList = floor.WaitingDownList;
        }
        else if(schedlue == eDirection.STANDBY)
        {
            waitingList = floor.WaitingUpList.Count >= floor.WaitingDownList.Count ? floor.WaitingUpList : floor.WaitingDownList;
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
