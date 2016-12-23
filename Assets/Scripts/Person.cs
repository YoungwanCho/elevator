using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour
{
    public int Weight { get { return Weight; } }

    private int weight = 0;

    public Person()
    {
        this.weight = Random.RandomRange(40, 100);
    }
}

