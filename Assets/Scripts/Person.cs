using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PERSON_STATE {WANT_UP = 0, WANT_DOWN = 1, TASK = 2} //@Description 0 = 해당층에서 일을 보는중, 1 = 올라가고 싶다, 2 = 내려가고 싶다

public class Person : MonoBehaviour
{
    public PERSON_STATE State{get {return this._state;} }
    public string Name {get {return this._name;} }
    public int TargetFloor{get {return this._targetFloor;} }


    private PERSON_STATE _state = PERSON_STATE.TASK;
    public Floor _currentFloorComponent = null;
    private string _name = string.Empty;
    private int _targetFloor = 0;

    static int indexCount = 0;

    public void Initialize(int personIndex, Floor currentFlootComponent)
    {
        this._state = PERSON_STATE.TASK;
        this._name = string.Format("{0}번째사람", indexCount++);
        this.gameObject.name = this._name;
        this._targetFloor = 0;
    }

    public void UpdateFloorInstance(Floor currentFloor)
    {
        if (currentFloor != null)
        {
            this._currentFloorComponent = currentFloor;

        }
        else
        {
            this._currentFloorComponent = null;
        }
    }
    
    public void EnterFloor(Floor currentFloorComponent)
    {
        this.UpdateFloorInstance(currentFloorComponent);
        currentFloorComponent.EnterFloorPerson(this);
        Action();
    }

    public void ExitFloor(Floor currentFloorComponent)
    {
        currentFloorComponent.ExitFloorPerson(this);
        UpdateFloorInstance(null);
    }

    private void Action()
    {
        StartCoroutine("ArtificialIntelligence");
    }

    private IEnumerator ArtificialIntelligence()
    {
        switch(this._state)
        {
            case PERSON_STATE.TASK :
                yield return Task();
                break;
            case PERSON_STATE.WANT_DOWN :
            case PERSON_STATE.WANT_UP:
                Debug.Log(string.Format("{0}번째 사람이 {1}층에서 {2}층으로 {3}상태입니다.", this.Name,  this._currentFloorComponent.FloorIndex, this._targetFloor, this._state));
                _currentFloorComponent.WantExitFloorPerson(this);
                yield return null;
                break;
        }        
    }

    private IEnumerator Task()
    {
        Debug.Log(string.Format("{0}은 {1}층에서 볼일을 보고 있다", this._name, this._currentFloorComponent.FloorIndex));
        yield return new WaitForSeconds(Random.Range(10, 20));
        this.WhereToGo();        
    }

    private void WhereToGo()
    {
        int randomIndex = 0;
        List<int> floorList = null;

        floorList =  GVallyPlaza.Instance.GetMovableFloorList(this._currentFloorComponent.FloorIndex);
        randomIndex = Random.Range(0, floorList.Count);
        
        this._targetFloor = floorList[randomIndex];

        if(this._targetFloor > this._currentFloorComponent.FloorIndex)
            this._state = PERSON_STATE.WANT_UP;
        else if(this._targetFloor < this._currentFloorComponent.FloorIndex)
            this._state = PERSON_STATE.WANT_DOWN;

        Action();
    }
}

