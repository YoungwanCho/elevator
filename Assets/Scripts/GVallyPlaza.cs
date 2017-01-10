using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GVallyPlaza : UnitySingleton<GVallyPlaza>
{
    public Transform floorParent_ = null;
    public Transform elevatorParent_ = null;

    private Floor[] floorComponentArr = new Floor[MAX_FLOOR_COUNT];
    private Elevator[] elevatorComponentArr = new Elevator[MAX_ELEVATOR_COUNT];
    private Vector3[] gateFixedPos = new Vector3[MAX_GATE_COUNT];
    private float[] floorHeightArr = new float[MAX_FLOOR_COUNT];

    public const int FLOOR_HEIGHT_INTERVAL = 1;
    public const int MAX_FLOOR_COUNT = 20;
    public const int MAX_GATE_COUNT = 8;
    public const int MAX_ELEVATOR_COUNT = 1;
    public const float ELEVATOR_SPEED = 2;
    public static WaitForSeconds GateOperationSec = null; //@TODO: 초단위가 아니라 실제 움직이 필요한 시간을 계산하는 방식으로 수정
    public static WaitForSeconds WaitForTheDoorToOpenSec = null;

    private const float GATE_POS_Z_MAX = 4.0f;
    private const float GATE_POS_Z_MIN = -4.0f;
    private const float GATE_POS_X_MAX = 5.5f;          
    private const float GATE_POS_X_MIN = -5.5f;

    public void Awake()
    {
        Application.runInBackground = true;
    } 

    public void Start()
    {
        
        GateOperationSec = new WaitForSeconds(1.0f);
        WaitForTheDoorToOpenSec = new WaitForSeconds(2.0f);
        BuildingPlaza();
    }

    public int OperationElevator(int floorIndex)
    {
        int elevatorIndex = 0;

        // 엘레베이터 오퍼레이팅 명령

        elevatorComponentArr[elevatorIndex].OrderedToWork(floorIndex);


        return elevatorIndex;
    }

    public Floor GetFloorComponent(int floorIndex)
    {
        return floorComponentArr[floorIndex];
    }

    /*@breif 현재 층을 제외한 갈수 있는 층의 리스트를 전달함
     */
    public List<int> GetMovableFloorList(int currentFloor)
    {
        //TODO: 나중에 저층부 고층부 모드를 나눠야 함
        List<int> list = new List<int>();

        for (int i = 0; i < floorComponentArr.Length; i++)
        {
            if (floorComponentArr[i].FloorIndex == currentFloor)
                continue;

            list.Add(floorComponentArr[i].FloorIndex);
        }
        return list;
    }
    /* @brief
     * 엘레베이터의 정지 유무와 상관없이, 현재 위치를 가져올수있는 함수
     * 알고리즘이 간단한게 더 좋은 방법이 있으면 그 때 교체 하자
     **/
    public int GetFlootValueFromHeight(float height)
    {
        int result = 0;

        for(int i=0; i<floorHeightArr.Length; i++)
        {
            if (floorHeightArr[i] <= i)
            {
                result = i;
                break;
            }
        }

        return result;
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
        for (int i = 0; i < MAX_ELEVATOR_COUNT; i++)
        {
            elevatorComponentArr[i] = FactoryBehavior.Instantiate<Elevator>("Prefabs/Elevator", elevatorParent_, gateFixedPos[i], Quaternion.identity, Vector3.one, string.Format("{0}Unit", i + 1));
            elevatorComponentArr[i].Initialize(i);
        }
    }

    private void CreateFloor()
    {
        GameObject baseFloorPrefab = Resources.Load<GameObject>("Prefabs/Floor");
        for (int i = 0; i < MAX_FLOOR_COUNT; i++)
        {
            floorComponentArr[i] = FactoryBehavior.Instantiate<Floor>("Prefabs/Floor", floorParent_, new Vector3(0.0f, floorHeightArr[i], 0.0f), Quaternion.identity, Vector3.one, string.Format("{0}F", i));
            floorComponentArr[i].Initialize(gateFixedPos, i);
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
