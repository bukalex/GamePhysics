using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMotion : MonoBehaviour
{
    [SerializeField]
    private float frequency = 1;
    [SerializeField]
    private float amplitude = 1; 

    void FixedUpdate()
    {
        transform.position = new Vector3(GetX(), GetY(), transform.position.z);
    }

    private float GetX()
    {
        return transform.position.x + (-Mathf.Sin(Time.time * frequency)) * frequency * amplitude * Time.fixedDeltaTime;
    }

    private float GetY()
    {
        return transform.position.y + Mathf.Cos(Time.time * frequency) * frequency * amplitude * Time.fixedDeltaTime;
    }
}
