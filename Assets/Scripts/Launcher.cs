using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    [SerializeField]
    private float launchAngle = 30;
    [SerializeField]
    private float launchSpeed = 3;

    private Vector3 velocity = Vector3.zero;
    private bool isLaunched = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            velocity = new Vector3(
                Mathf.Cos(launchAngle * Mathf.Deg2Rad), 
                Mathf.Sin(launchAngle * Mathf.Deg2Rad), 
                0);
            velocity *= launchSpeed;

            isLaunched = true;
        }
    }

    void FixedUpdate()
    {
        if (!isLaunched) return;

        velocity += Physics.gravity * Time.fixedDeltaTime;
        transform.position += velocity * Time.fixedDeltaTime;
    }
}
