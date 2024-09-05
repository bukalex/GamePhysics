using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisDraw : MonoBehaviour
{
    [SerializeField]
    private float axisLength = 2;

    void Update()
    {
        Debug.DrawLine(transform.position, transform.position + Vector3.forward * axisLength, Color.blue);
        Debug.DrawLine(transform.position, transform.position + Vector3.up * axisLength, Color.green);
        Debug.DrawLine(transform.position, transform.position + Vector3.right * axisLength, Color.red);
    }
}
