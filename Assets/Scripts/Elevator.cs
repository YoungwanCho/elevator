using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorCommand
{
    private Vector3 _targetWorldPos = Vector3.zero;
    private Vector3 _direction = Vector3.zero;
    private System.Action<int, int> _callBack = null;
    private int _targetFloorIndex = 0;
    private List<Person> passengerList = new List<Person>(); // 탑승인원
 
    public Vector3 TargetWorldPos { get { return this._targetWorldPos; } }
    public Vector3 Direction { get { return this._direction; } }
    public System.Action<int, int> CallBack { get { return this._callBack; } }
    public int TargetFloorIndex { get { return _targetFloorIndex; } }

    public ElevatorCommand(int targetFloorIndex, Vector3 targetWorldPos, Vector3 direction, System.Action<int, int> callBack)
    {
        this._targetFloorIndex = targetFloorIndex;
        this._targetWorldPos = targetWorldPos;
        this._direction = direction;
        this._callBack = callBack;
    }
}

public class Elevator : MonoBehaviour
{
    private enum eState {STANDBY = 0, STOP = 1, UP = 2, DOWN = 3};
    private eState _state = eState.STANDBY;
    private List<ElevatorObserver> observerList = new List<ElevatorObserver>();
    private int elevatorIndex;
    private int operationCount = 0;

    public void SetElevator(int index)
    {
        this.elevatorIndex = index;
        Debug.Log(string.Format("엘레베이터 생성 인덱스 할당 : {0}호기", index));
    }

    public void RegisterObserver(ElevatorObserver o)
    {
        observerList.Add(o);
    }

    public void UnregisterObserver(ElevatorObserver o)
    {
        observerList.Remove(o);
    }

    public void UpdateElevatorInfo()
    {
        for (int i = 0; i < observerList.Count; i++)
            observerList[i].UpdateIndigator(0);
    }

    public void MoveToDestination(int targetFloorIndex, Vector3 targetPos, System.Action<int, int> callBack)
    {
        Debug.Log(string.Format("{0}호기, 목표:{1}층, 운행카운트:{2}", this.elevatorIndex, targetFloorIndex, operationCount));   


        Vector3 direction = this.GetDirectionToGo(this.transform.position, targetPos);
        ElevatorCommand command = new ElevatorCommand(targetFloorIndex, targetPos, direction, callBack);
        IEnumerator coroutine = null;
        coroutine = Moving(command);
        StopCoroutine(coroutine);
        StartCoroutine(coroutine);
    }

    private IEnumerator Moving(ElevatorCommand command)
    {
        float dist = 0.0f;

        do
        {
            dist = Vector3.Distance(this.transform.position, command.TargetWorldPos);
            this.transform.Translate(command.Direction * Time.deltaTime);
            yield return null;
        } while (dist >= 0.1f);
        operationCount++;
        this.transform.position = command.TargetWorldPos;
        if (command.CallBack != null)
            command.CallBack(elevatorIndex, command.TargetFloorIndex);
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

    private void GateOperation()
    {
        //닫 힐건지 열린건지 판단
    }

    private IEnumerator GateOpen()
    {
        //TODO: 문이 열릴수 있는 조건 1. 포지션이 정확한가 2.엘레베이터는 확실히 정지한 상태인가?
        Debug.Log(string.Format("문이 닫힙니다."));
        yield return GVallyPlaza.GateOperationSec; // 시간으로 계산하기보다는 실제 닫히걸 계산 해줘야 한다, 닫히다가 열리는경우도 있기에 일단 시간으로
        Debug.Log(string.Format("철컥(닫힘)사운드"));
    }

    private IEnumerator GateClose()
    {
        //TODO: 문이 닫힐수 있는 조건 1.초과탑승이지 않는가? 2.게이트센서에 걸리지 않는가?
        Debug.Log(string.Format("문이 닫힙니다."));
        yield return GVallyPlaza.GateOperationSec; // 시간으로 계산하기보다는 실제 닫히걸 계산 해줘야 한다, 닫히다가 열리는경우도 있기에 일단 시간으로
        Debug.Log(string.Format("철컥(닫힘)사운드"));
    }

    private bool CheckWeightCapacity()
    {
        return true;
    }

    private void GetInPerson(Person person)
    {

    }

    private void GetOutPerson(Person person)
    {

    }

    private bool GoodFitDocking()
    {
        return true;
    }
}
