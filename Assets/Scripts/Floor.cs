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
    
    public Vector3 GetGateWorldPosition(int gateIndex)
    {
        return gateWorldPostion[gateIndex];
    }

    public void InitializeFloor(Vector3[] gatePosArr)
    {
        CreateGateObject(gatePosArr);
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
