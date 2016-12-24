using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorGate : MonoBehaviour, ElevatorObserver
{
    public int gateIndex = 0;

    public GameObject indicatorLightUp_ = null;
    public GameObject indicatorLightDown_ = null;
    public GameObject indicatorElevatorLocation_ = null;

    public void OnClickDownButton()
    {

    }

    public void OnClickUpButton()
    {

    }

    public void UpdateIndicatorLightUP()
    {

    }

    public void UpdateIndicatorLightDown()
    {

    }

    public IEnumerator FlickerIndicatorLightUp()
    {
        yield return null;
    }

    public IEnumerator FlickerIndicatorLightDown()
    {
        yield return null;
    }

    public void UpdateIndigator(int currentElevatorLocation)
    {

    }

    public void UpdateIndicatorElevatorLocation(int locationValue)
    {

    }
}
