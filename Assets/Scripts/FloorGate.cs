using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* @Brief:층에서 윈풋버튼의 입력을 처리한다 
 * @TODO: 엘레베이터의 문과 도킹이 잘되면 열리는것 가지 구현 예정
 */
public class FloorGate : MonoBehaviour, ElevatorObserver, FloorObserver
{
    public GameObject indicatorLightUp_ = null;
    public GameObject indicatorLightDown_ = null;
    public GameObject indicatorElevatorLocation_ = null;

    private Floor _currentFloorComponent = null;
    private bool _isOnButtonUp = false;
    private bool _isOnButtonDown = false;
    private int _gateIndex = 0;

    public void Initialize(Floor currentFloorComponent, int gateIndex)
    {
        if (this._currentFloorComponent != null)
            return;

        this._currentFloorComponent = currentFloorComponent;
        this._gateIndex = gateIndex;
        this._isOnButtonUp = false;
        this._isOnButtonDown = false;
    }

    public void OnClickUpButton()
    {
        
    }

    public void OnClickDownButton()
    {

    }

    public void UpdateButtonUp(bool isOn)
    {
        this._isOnButtonUp = isOn;
    }

    public void UpdateButtonDown(bool isOn)
    {
        this._isOnButtonDown = isOn;
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
