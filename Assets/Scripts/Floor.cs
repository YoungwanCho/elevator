using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    public Transform gateParent_ = null;
    public GameObject[] gateObject = null;
    public Vector3[] gatePosition = null;
    public Vector3[] gateWorldPostion = null;

    private const float gatePositionZmax = 4.0f;
    private const float gatePositionZmin = -4.0f;
    private const float gatePositionXleft = -5.5f;
    private const float gatePositionXright = 5.5f;

    private const int MAX_GATE_COUNT = 8;

    public Vector3 GetGatePoision(int gateIndex)
    {
        return gatePosition[gateIndex];
    }

    public Vector3 GetGateWorldPosition(int gateIndex)
    {
        return gateWorldPostion[gateIndex];
    }

    public void InitializeFloor()
    {
        gateObject = new GameObject[MAX_GATE_COUNT];
        gatePosition = new Vector3[MAX_GATE_COUNT];
        gateWorldPostion = new Vector3[MAX_GATE_COUNT];
        
        CreateGateObject();
    }

    private void CreateGateObject()
    {        
        for (int i = 0; i < MAX_GATE_COUNT; i++)
        {
            gateObject[i] = new GameObject();
            gateObject[i].transform.parent = gateParent_;
            gatePosition[i] = this.CalculateGateLocalPosition(i);
            gateObject[i].transform.localPosition = gatePosition[i];
            gateWorldPostion[i] = gateObject[i].transform.position;
            gateObject[i].transform.localRotation = Quaternion.identity;
            gateObject[i].transform.localScale = Vector3.one;
            gateObject[i].name = string.Format("{0}Gate", i + 1);
        }
    }	

    private Vector3 CalculateGateLocalPosition(int gateIndex)
    {
        float xPos = 0.0f;
        float yPos = 0.0f;
        float zPos = 0.0f;
        int halfCount = MAX_GATE_COUNT / 2;
        int rowIndex = gateIndex % halfCount;
        bool isLeft = (gateIndex < halfCount);
        int rowInterval = 8 / halfCount; // TODO: 1호기와 4호기의 x값의 차이인데, 나중에 동적으로 구할수있도록 구현

        xPos = isLeft ? gatePositionXleft : gatePositionXright;
        zPos = gatePositionZmin + (rowIndex * rowInterval);

        return new Vector3(xPos, yPos, zPos);
    }
}
