using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    public List<Person> WaitingDownList { get { return waitingDownPersonList; } }
    public List<Person> WaitingUpList { get { return waitingUpPersonList; } }
    public int FloorIndex { get { return _floorIndex; } }

    public Transform gateParent_ = null;
    public Transform waittingLine_ = null;

    public FloorGate[] gateComponentArr = null;
    public Vector3[] gatePositionArr = null;
    public Vector3[] gateWorldPostionArr = null;

    private List<FloorObserver> observerList = new List<FloorObserver>();
    private List<Person> onTheFloorPersonList = new List<Person>();
    private List<Person> waitingDownPersonList = new List<Person>();
    private List<Person> waitingUpPersonList = new List<Person>();

    private int _floorIndex = 0;

    public void Initialize(Vector3[] gatePosArr, int floorIndex)
    {


        onTheFloorPersonList.Clear();
        waitingDownPersonList.Clear();
        waitingUpPersonList.Clear();
        this._floorIndex = floorIndex;
        CreateGateObject(gatePosArr);
        CreatePerson();
        Debug.Log(string.Format("FloorIndex : {0}, Initialize", this._floorIndex));
    }

    #region observer
    public void RegisterObserver(FloorObserver o)
    {
        observerList.Add(o);
    }

    public void UnregisterObserver(FloorObserver o)
    {
        observerList.Remove(o);
    }

    public void UpdateButtonUp(bool isOn)
    {
        for (int i = 0; i < observerList.Count; i++)
            observerList[i].UpdateButtonUp(isOn); // bool값 판단해줄것
    }

    public void UpdateButtonDown(bool isOn)
    {
        for (int i = 0; i < observerList.Count; i++)
            observerList[i].UpdateButtonUp(isOn);
    }
    #endregion

    /* @brief
 * 엘레베이터가 도착하면 호출될 함수
 */
    public void ElevatorArrives(Elevator elevator)
    {
        /*
         * @TODO:
         * 0.엘레베이터 안에서 내려야할 인원이 있으면 내린다 내릴 인원 리스트로 받고
         * 1.엘레베이터의 방향에 따라
         * 2.승차가능한 인원만큼 대기자들을 타고(리스트로 넘기고) 출발
         * */
    }

    public void EnterFloorPerson(Person person)
    {
        onTheFloorPersonList.Add(person);
        Debug.Log(string.Format("{0}이 {1}층에 들어왔습니다.", person.Name, this.FloorIndex));
        UpdateWaitingLine();
    }

    public void ExitFloorPerson(Person person)
    {
        if(person.State == PERSON_STATE.WANT_DOWN)
        {
            waitingDownPersonList.Remove(person);
        }
        else if( person.State == PERSON_STATE.WANT_UP)
        {
            waitingUpPersonList.Remove(person);
        }
        onTheFloorPersonList.Remove(person);
        Debug.Log(string.Format("{0}이 {1}층에서 나갔습니다.", person.Name, this.FloorIndex));
        UpdateWaitingLine();
    }

    public void WantExitFloorPerson(Person person)
    {
        if (!onTheFloorPersonList.Contains(person))
        {
            Debug.Log("현재 층에 존재 하지 않는 사람이 탈출을 시도합니다.");
            return;
        }

        switch (person.State)
        {
            case PERSON_STATE.WANT_DOWN:
                waitingDownPersonList.Add(person);
                break;
            case PERSON_STATE.WANT_UP:
                waitingUpPersonList.Add(person);
                break;
        }

        CallingElevator();
    }

    private void CallingElevator()
    {
        for(int i=0; i<waitingDownPersonList.Count; i++)
        {
            Debug.Log(string.Format("{0}층에서 Down 오더를 넣었다", this._floorIndex));
            GVallyPlaza.Instance.AddOrderListDown(this._floorIndex);
        }

        for(int i=0; i<waitingUpPersonList.Count; i++)
        {
            Debug.Log(string.Format("{0}층에서 Up 오더를 넣었다", this._floorIndex));
            GVallyPlaza.Instance.AddOrderListUp(this._floorIndex);
        }
    }

    private void UpdateWaitingLine()
    {
        Debug.LogWarning(string.Format("UpdateWaitingLine FloorIndex : {0}, OnThePersonCount : {1}", this._floorIndex, this.onTheFloorPersonList.Count));
        int waitlinecount = onTheFloorPersonList.Count;

        for (int i = 0; i < waitlinecount; i++)
        {
            onTheFloorPersonList[i].transform.parent = waittingLine_;
            onTheFloorPersonList[i].transform.localPosition = new Vector3(1 * i, 0.0f, 0.0f);
            Debug.LogWarning(string.Format("Name : {0}, localPosition : {1}", onTheFloorPersonList[i].name, onTheFloorPersonList[i].transform.localPosition));
        }
    }
 
    //public Vector3 GetGateWorldPosition(int gateIndex)
    //{
    //    return gateWorldPostionArr[gateIndex];
    //}

    public Transform GetGateTranform(int gateIndex)
    {
        return gateComponentArr[gateIndex].transform;
    }

    private void CreateGateObject(Vector3[] gatePosArr)
    {
        gatePositionArr = new Vector3[GVallyPlaza.MAX_GATE_COUNT];
        gateWorldPostionArr = new Vector3[GVallyPlaza.MAX_GATE_COUNT];
        gateComponentArr = new FloorGate[GVallyPlaza.MAX_GATE_COUNT];
        Vector3 gateScale = new Vector3(0.2f, 1.0f, 1.0f);
        for (int i = 0; i < GVallyPlaza.MAX_GATE_COUNT; i++)
        {
            gateComponentArr[i] = FactoryBehavior.Instantiate<FloorGate>("Prefabs/FloorGate", gateParent_, gatePosArr[i], Quaternion.identity, gateScale, string.Format("{0}Gate", i + 1));
            gateComponentArr[i].Initialize(this, i);
            gatePositionArr[i] = gateComponentArr[i].transform.localPosition;
            gateWorldPostionArr[i] = gateComponentArr[i].transform.position;
        }
    }

    private void CreatePerson()
    {
        if (_floorIndex != 0)
            return;

        Person personInstance = null;

        for(int i=0; i<10; i++)
        {
            personInstance = FactoryBehavior.Instantiate<Person>("Prefabs/Person", null, Vector3.zero, Quaternion.identity, Vector3.one, string.Format("{0}층의 {1}번째 사람", _floorIndex, i));
            personInstance.Initialize(i, this);
            personInstance.EnterFloor(this);
        }
    }
}
