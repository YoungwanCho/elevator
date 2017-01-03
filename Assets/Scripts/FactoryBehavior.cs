using UnityEngine;
using System.Collections;

public class FactoryBehavior : MonoBehaviour
{
    public static T Instantiate<T>(string ResourcePath, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale, string name = "", bool isLocal = true)
    {
        GameObject original = Resources.Load<GameObject>(ResourcePath);
        GameObject clone = GameObject.Instantiate<GameObject>(original, parent) as GameObject;
        if(isLocal)
        {
            clone.transform.localPosition = position;
            clone.transform.localRotation = rotation;
            clone.transform.localScale = scale;            
        }
        else
        {
            clone.transform.position = position;
            clone.transform.rotation = rotation;
            clone.transform.localScale = scale; // 무조건 로컬 스케일
        }
        
        if (name != string.Empty)
        {
            clone.name = name;
        }

        T component = clone.GetComponent<T>();
        return component;
    }
}

