using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce : MonoBehaviour
{
    [SerializeField]
    private Vector3 velocity = Vector3.zero;
    [SerializeField]
    private float ground = 0;

    void FixedUpdate()
    {
        velocity += Physics.gravity * Time.fixedDeltaTime;
        transform.position += velocity * Time.fixedDeltaTime;

        if (transform.position.y <= ground && Vector3.Dot(velocity, Physics.gravity) > 0) velocity *= -1;
    }
}
