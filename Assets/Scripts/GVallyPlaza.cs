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

    private List<int> _orderListDown = new List<int>();
    private List<int> _orderListUp = new List<int>();

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
        _orderListDown.Clear();
        _orderListUp.Clear();
        GateOperationSec = new WaitForSeconds(1.0f);
        WaitForTheDoorToOpenSec = new WaitForSeconds(2.0f);
        BuildingPlaza();
    }

    public void AddOrderListUp(int orderFloorIndex)
    {
        if(_orderListUp.Contains(orderFloorIndex))
        {
            return;
        }
        this._orderListUp.Add(orderFloorIndex);
        this.OrderProcess();
    }

    public void AddOrderListDown(int orderFloorIndex)
    {
        if(_orderListDown.Contains(orderFloorIndex))
        {
            return;
        }
        this._orderListDown.Add(orderFloorIndex);
        this.OrderProcess();
    }

    public void ElevatorStranBy(Elevator elevator)
    {
        Debug.Log(string.Format("{0}호기 엘레베이터가 스탠바이 상태라고 연락을 줬다", elevator.ElevatorIndex));
        this.OrderProcess();
    }

    private void OrderProcess()
    {
        Elevator elevator = null;
        bool isTurning = false;
        int upCount = this._orderListUp.Count;
        int downCount = this._orderListDown.Count;


        int orderFloorIndex = 0;

        for(int i=0; i<this._orderListUp.Count; i++)
        {
            orderFloorIndex = this._orderListUp[i];
            elevator = LookForTheBestElavator(Elevator.eDirection.UP, orderFloorIndex);
            if(elevator != null)
            {
                isTurning = IsNeedToTurning(elevator, Elevator.eDirection.UP, orderFloorIndex);
                this._orderListUp.Remove(orderFloorIndex);
                elevator.OrderedToWork(isTurning, Elevator.eDirection.UP, orderFloorIndex);
            }
        }

        for (int i=0; i<this._orderListDown.Count; i++)
        {
            orderFloorIndex = this._orderListDown[i];
            elevator = LookForTheBestElavator(Elevator.eDirection.DOWN, orderFloorIndex);
            if(elevator != null)
            {
                isTurning = IsNeedToTurning(elevator, Elevator.eDirection.DOWN, orderFloorIndex);
                this._orderListDown.Remove(orderFloorIndex);
                elevator.OrderedToWork(isTurning, Elevator.eDirection.DOWN, orderFloorIndex);
            }
        }
    }

    public bool IsNeedToTurning(Elevator elevator, Elevator.eDirection wantDirection,  int orderFloorIndex)
    {
        bool isTurning = false;
        switch (wantDirection)
        {
            case Elevator.eDirection.UP:
                {
                    isTurning = (elevator.CurrentFloorIndex <= orderFloorIndex) ? false : true;
                    break;
                }
            case Elevator.eDirection.DOWN:
                {
                    isTurning = (elevator.CurrentFloorIndex >= orderFloorIndex) ? false : true;
                    break;
                }
        }
        return isTurning;
    }

    private Elevator LookForTheBestElavator(Elevator.eDirection direction, int orderFloorIndex)
    {
        List<Elevator> elevatorList = new List<Elevator>();
        Elevator elevator = null;
        for(int i=0; i<elevatorComponentArr.Length; i++)
        {
            elevator = elevatorComponentArr[i];
            if (elevator.IsCanStopFloor(direction, orderFloorIndex))
            {
                break;
            }
            else
            {
                elevator = null;
            }
        }
        Debug.Log(string.Format("{0}층으로 운행이 적합한 엘레베이터 {1}호기", orderFloorIndex, elevator == null? "None" : elevator.name));
        return elevator; // null이 리턴 될 경우도 생각해야한다
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
