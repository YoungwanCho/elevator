using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    public void SetElevatorPosition(Vector3 pos)
    {
        this.transform.localPosition = pos;
    }
}
