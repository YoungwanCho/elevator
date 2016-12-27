using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PERSON_STATE {TASK = 0, ,WANT_UP, WANT_DOWN} //@Description 0 = 해당층에서 일을 보는중, 1 = 올라가고 싶다, 2 = 내려가고 싶다

public class Person : MonoBehaviour
{
    public PERSON_STATE State{get {return this._state;} }
    public string Name {get {return this._name;} }
    public int CurrentFloor{get {return this._currentFloor;} }
    public int TargetFloor{get {return this._targetFloor;} }
    public int Weight {get {return this._weight;} }

    private PERSON_STATE _state = PERSON_STATE.TASK;
    private string _name = string.Empty;
    private int _currentFloor = 0;
    private int _targetFloor = 0;
    private int _weight = 0;

    public Person(int pIndex)
    {
        this._state = PERSON_STATE.TASK;
        this._name = string.Format("{0}번째사람", pIndex);
        this._weight = Random.RandomRange(40, 100);
        this._currentFloor = 0;
        this._targetFloor = 0;        
    }

    private void Action()
    {
        StartCoroutine(ArtificialIntelligence());
    }

    private IEnumerator ArtificialIntelligence()
    {
        switch(this._state)
        {
            case PERSON_STATE.TASK :
                yield return Task();
                break;
            case PERSON_STATE.WANT_DOWN :
                yield return null;
                break;
            case PERSON_STATE.WANT_UP:
                yield return null;
                break;
        }        
    }

    private IEnumerator Task()
    {
        Debug.Log(string.Format("{0}은 {1}층에서 볼일을 보고 있다", this._name, this._currentFloor));
        yield return new WaitForSeconds(Random.Range(10, 20));
        this.WhereToGo();        
    }

    private void WhereToGo()
    {
        int randomIndex = 0;
        List<int> floorList = null;

        floorList =  GVallyPlaza.Instance.GetMovableFloorList(this._currentFloor);
        randomIndex = Random.Range(0, floorList.Count);
        
        this._targetFloor = floorList[randomIndex];

        if(this._targetFloor > this._currentFloor)
            this._state = PERSON_STATE.WANT_UP;
        else if(this._targetFloor < this._currentFloor)
            this._state = PERSON_STATE.WANT_DOWN;
    }
}

