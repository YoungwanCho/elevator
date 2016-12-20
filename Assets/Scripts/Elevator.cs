using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorCommand
{
    private Vector3 _targetWorldPos = Vector3.zero;
    private Vector3 _direction = Vector3.zero;
    private System.Action<int, int> _callBack = null;
    private int _targetFloorIndex = 0;

    
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

    private int elevatorIndex;
    private int operationCount = 0;

    public void SetElevator(int index)
    {
        this.elevatorIndex = index;
        Debug.Log(string.Format("엘레베이터 생성 인덱스 할당 : {0}호기", index));
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
}
