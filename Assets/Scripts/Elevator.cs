using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    private enum eState {STANDBY = 0, STOP = 1, UP = 2, DOWN = 3};
    private eState _direction = eState.STANDBY;   

    public void AWake()
    {
    }

    public void Start()
    {
    }

    public void SetElevatorPosition(Vector3 pos)
    {
        this.transform.localPosition = pos;
    }

    public void MoveToDestination(Vector3 targetPos)
    {
        IEnumerator coroutine = null;
        coroutine = Moving(targetPos);
        StopCoroutine(coroutine);
        StartCoroutine(coroutine);
    }

    private IEnumerator Moving(Vector3 targetWorldPos)
    {
        float dist = Vector3.Distance(this.transform.position, targetWorldPos);
        do
        {
            dist = Vector3.Distance(this.transform.position, targetWorldPos);
            this.transform.Translate(Vector3.up * Time.deltaTime);
            yield return null;
        } while (dist >= 0.1f);
        this.transform.position = targetWorldPos;
    }    
}
