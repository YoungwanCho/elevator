using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GVallyPlaza : UnitySingleton<GVallyPlaza>
{
    public Transform floorParent_ = null;
    public Transform elevatorParent_ = null;

    private Floor[] floorInstanceArr = new Floor[MAX_FLOOR_COUNT];
    private Elevator[] elevatorInstanceArr = new Elevator[MAX_ELEVATOR_COUNT];
    private Vector3[] gateFixedPos = new Vector3[MAX_GATE_COUNT];
    private float[] floorHeightArr = new float[MAX_FLOOR_COUNT];

    public const int FLOOR_HEIGHT_INTERVAL = 1;
    public const int MAX_FLOOR_COUNT = 20;
    public const int MAX_GATE_COUNT = 8;
    public const int MAX_ELEVATOR_COUNT = 8;

    private const float GATE_POS_Z_MAX = 4.0f;
    private const float GATE_POS_Z_MIN = -4.0f;
    private const float GATE_POS_X_MAX = 5.5f;
    private const float GATE_POS_X_MIN = -5.5f;

    public void Start()
    {
        BuildingPlaza();

        ElevatorOperation();
    }
        
    private void ElevatorOperation()
    {
        int targetFloorIndex = Random.RandomRange(0, 20);
        Vector3 targetPos = Vector3.zero;

        for (int i = 0; i < elevatorInstanceArr.Length; i++)
        {
            targetPos = floorInstanceArr[targetFloorIndex].GetGateWorldPosition(i);
            Debug.Log(string.Format("{0}호기 : 목표층 좌표 : {1}", i, targetPos));
            elevatorInstanceArr[i].MoveToDestination(targetFloorIndex, targetPos, this.CallBackOperationEnd);
        }
    }

    public void CallBackOperationEnd(int elevatorIndex, int currentFloor)
    {
        int targetFloorValue = Random.RandomRange(0, 20); // 현재층을 제외하고 랜덤 값을 뽑을수 있도록 수정 

        Vector3 targetPos = floorInstanceArr[targetFloorValue].GetGateWorldPosition(elevatorIndex);
        elevatorInstanceArr[elevatorIndex].MoveToDestination(targetFloorValue, targetPos, this.CallBackOperationEnd);
    }
    
#region BuildGvallyPlaza
    /*
     * @brief 20개의 층, 8개의 엘레베이터 객체를 만든다.
     * */
    private void BuildingPlaza()
    {
        CalculateFloorFixedHieght();
        CalculateGateFixedPos();
        CreateFloor();
        CreateElevator();
    }

    private void CreateElevator()
    {
        GameObject baseElevatorPrefab = Resources.Load<GameObject>("Prefabs/Elevator");
        for (int i = 0; i < MAX_ELEVATOR_COUNT; i++)
        {
            Transform objTransform = GameObject.Instantiate<GameObject>(baseElevatorPrefab).transform;
            objTransform.parent = elevatorParent_;
            objTransform.localPosition = gateFixedPos[i];
            objTransform.localRotation = Quaternion.identity;
            objTransform.localScale = Vector3.one;
            objTransform.name = string.Format("{0}Unit", i + 1);
            elevatorInstanceArr[i] = objTransform.GetComponent<Elevator>();
            elevatorInstanceArr[i].SetElevator(i);
        }
    }

    private void CreateFloor()
    {
        GameObject baseFloorPrefab = Resources.Load<GameObject>("Prefabs/Floor");
        for (int i = 0; i < MAX_FLOOR_COUNT; i++)
        {
            Transform objTransform = GameObject.Instantiate<GameObject>(baseFloorPrefab, new Vector3(0.0f, floorHeightArr[i], 0.0f), Quaternion.identity, floorParent_).transform;
            objTransform.name = string.Format("{0}F", i + 1);
            floorInstanceArr[i] = objTransform.GetComponent<Floor>();
            floorInstanceArr[i].InitializeFloor(gateFixedPos);
        }
    }

    private void CalculateGateFixedPos()
    {
        float xPos = 0.0f;
        float yPos = 0.0f;
        float zPos = 0.0f;

        int halfCount = GVallyPlaza.MAX_GATE_COUNT / 2;
        int rowInterval = MAX_GATE_COUNT / halfCount; // TODO: 1호기와 4호기의 x값의 차이인데, 나중에 동적으로 구할수있도록 구현
        int rowIndex = 0;
        bool isLeft = false;

        for (int i = 0; i < MAX_GATE_COUNT; i++)
        {
            rowIndex = i % halfCount;
            isLeft = (i < halfCount);
            xPos = isLeft ? GATE_POS_X_MIN : GATE_POS_X_MAX;
            zPos = GATE_POS_Z_MIN + (rowInterval * rowIndex);
            gateFixedPos[i] = new Vector3(xPos, yPos, zPos);
        }
    }

    private void CalculateFloorFixedHieght()
    {
        for (int i = 0; i < MAX_FLOOR_COUNT; i++)
        {
            floorHeightArr[i] = FLOOR_HEIGHT_INTERVAL * i;
        }
    }
#endregion
}
