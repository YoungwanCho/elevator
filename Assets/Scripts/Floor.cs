using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    public Transform gateParent_ = null;

    public GameObject[] gateObject = null;
    public FloorGate[] gateInstanceArr = null;
    public Vector3[] gatePosition = null;
    public Vector3[] gateWorldPostion = null;

    private List<Person> onTheFloorPersonList = new List<Person>();
    private List<Person> waitingDownPersonList = new List<Person>();
    private List<Person> waitingUpPersonList = new List<Person>();

    public void GetinFloorPerson(Person person)
    {
        onTheFloorPersonList.Add(person);
    }

    public void GetOutFloorPerson(Person person)
    {
        if (onTheFloorPersonList.Contains(person))
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
    }

    public void ElevatorArrives(Elevator elevator)
    {
        /*
         * @TODO:
         * 0.엘레베이터 안에서 내려야할 인원이 있으면 내린다 내릴 인원 리스트로 받고
         * 1.엘레베이터의 방향에 따라
         * 2.승차가능한 인원만큼 대기자들을 타고(리스트로 넘기고) 출발
         * */        
    }
 
    public Vector3 GetGateWorldPosition(int gateIndex)
    {
        return gateWorldPostion[gateIndex];
    }

    public void InitializeFloor(Vector3[] gatePosArr)
    {
        CreateGateObject(gatePosArr);
        onTheFloorPersonList.Clear();
        waitingDownPersonList.Clear();
        waitingUpPersonList.Clear();
    }

    private void CreateGateObject(Vector3[] gatePosArr)
    {
        gateObject = new GameObject[GVallyPlaza.MAX_GATE_COUNT];
        gatePosition = new Vector3[GVallyPlaza.MAX_GATE_COUNT];
        gateWorldPostion = new Vector3[GVallyPlaza.MAX_GATE_COUNT];
        gateInstanceArr = new FloorGate[GVallyPlaza.MAX_GATE_COUNT];

        GameObject baseGatePrefab = Resources.Load<GameObject>("Prefabs/FloorGate");
        for (int i = 0; i < GVallyPlaza.MAX_GATE_COUNT; i++)
        {
            gateObject[i] = GameObject.Instantiate<GameObject>(baseGatePrefab, gateParent_) as GameObject; // Instantiate에서 하번에 초기화 할려했더니 로컬||월드 포지션구분이 안됨, 아마도 월드포지션 초기화를 하는것 같음
            gateObject[i].transform.localPosition = gatePosArr[i];
            gateObject[i].transform.localRotation = Quaternion.identity;
            gateObject[i].transform.localScale = new Vector3(0.2f, 1.0f, 1.0f);
            gateWorldPostion[i] = gateObject[i].transform.position;
            gateObject[i].name = string.Format("{0}Gate", i + 1);
            gateInstanceArr[i] = gateObject[i].GetComponent<FloorGate>();
        }
    }
}
