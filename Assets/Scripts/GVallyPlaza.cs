using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GVallyPlaza : MonoBehaviour
{
    public Transform[] elevatorArr = new Transform[MAX_ELEVATOR_COUNT];

    public Transform floorParent_ = null; // this.gameobject;
    public Transform elevatorParent_ = null;
    public GameObject baseFloor_ = null;
    public GameObject baseElevator_ = null;

    public Floor[] floorInstanceArray = new Floor[MAX_FLOOR_COUNT];

    public const int FLOOR_HEIGHT_INTERVAL = 1;
    public const int MAX_FLOOR_COUNT = 20;
    public const int MAX_ELEVATOR_COUNT = 8;

    public void Start()
    {
        CreateFloor();
        CreateElevator();        

        for(int i=0; i<MAX_ELEVATOR_COUNT; i++)
        {
            InitElevatorPosition(0, i);
        }
    }

    private void InitElevatorPosition(int floorIndex, int elevatorIndex)
    {
        elevatorArr[elevatorIndex].GetComponent<Elevator>() .SetElevatorPosition(floorInstanceArray[floorIndex].GetGatePoision(elevatorIndex));
    }

    private void CreateElevator()
    {
        for(int i=0; i<MAX_ELEVATOR_COUNT; i++)
        {
            Transform objTransform = GameObject.Instantiate<GameObject>(baseElevator_).transform;
            objTransform.parent = elevatorParent_;
            objTransform.localPosition = Vector3.zero;
            objTransform.localRotation = Quaternion.identity;
            objTransform.localScale = Vector3.one;
            objTransform.name = string.Format("{0}Unit", i + 1);
            elevatorArr[i] = objTransform;
        }
    }

    private void CreateFloor()
    {
        for(int i=0; i<MAX_FLOOR_COUNT; i++)
        {
            Transform objTransform = GameObject.Instantiate<GameObject>(baseFloor_).transform;
            objTransform.parent = floorParent_;
            objTransform.localPosition = new Vector3(0.0f, FLOOR_HEIGHT_INTERVAL * i, 0.0f);
            objTransform.localRotation = Quaternion.identity;
            objTransform.localScale = Vector3.one;
            objTransform.name = string.Format("{0}F", i+1);

            floorInstanceArray[i] = objTransform.GetComponent<Floor>();
            floorInstanceArray[i].InitializeFloor();
        }
    }
}
