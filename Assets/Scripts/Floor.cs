using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    public Transform gateParent_ = null;
    public GameObject[] gateObject = null;
    public Vector3[] gatePosition = null;

    private const float gatePositionZmax = 4.0f;
    private const float gatePositionZmin = -4.0f;
    private const float gatePositionXleft = -5.5f;
    private const float gatePositionXright = 5.5f;

    private const int MAX_GATE_COUNT = 8;

    public Vector3 GetGatePoision(int gateIndex)
    {
        return gatePosition[gateIndex];
    }

    public void InitializeFloor()
    {
        SetPositionGateObject();
    }

    private void SetPositionGateObject()
    {
        gatePosition = new Vector3[MAX_GATE_COUNT];
        
        for (int i = 0; i < MAX_GATE_COUNT; i++)
        {
            GameObject gateObject = new GameObject();
            gateObject.transform.parent = gateParent_;
            gatePosition[i] = this.GetGateLocalPosition(i);
            gateObject.transform.localPosition = gatePosition[i];
            gateObject.transform.localRotation = Quaternion.identity;
            gateObject.transform.localScale = Vector3.one;
            gateObject.name = string.Format("{0}Gate", i + 1);
        }
    }	

    private Vector3 GetGateLocalPosition(int gateIndex)
    {
        float xPos = 0.0f;
        float yPos = 0.0f;
        float zPos = 0.0f;
        int halfCount = MAX_GATE_COUNT / 2;
        int rowindex = gateIndex % halfCount;
        bool isLeft = (gateIndex < halfCount);
        int rowInterval = 8 / halfCount; // TODO: 1호기와 4호기의 x값의 차이인데, 나중에 동적으로 구할수있도록 구현

        xPos = isLeft ? gatePositionXleft : gatePositionXright;
        zPos = gatePositionZmin + (gateIndex * rowindex);

        return new Vector3(xPos, yPos, zPos);
    }
}
