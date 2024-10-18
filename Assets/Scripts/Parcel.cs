using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parcel : MonoBehaviour, IBeginOverlap
{
    private PhysicsBody body;
    private Vector3 initialPosition;
    private float flightTime = 0;
    private int testIndex;

    private void Awake()
    {
        body = GetComponent<PhysicsBody>();
        initialPosition = body.Position;
    }

    private void FixedUpdate()
    {
        flightTime += Time.fixedDeltaTime;
    }

    public void SetVelocity(Vector3 velocity, int testIndex)
    {
        body.Velocity = velocity;
        this.testIndex = testIndex;
    }

    public void OnBeginOverlap(PhysicsShape otherShape, HitResult hitResult)
    {
        if (!body.enabled) return;

        if (otherShape.CompareTag("Ground") && body.Velocity.y <= 0)
        {
            Cannon.ranges[testIndex] = (body.Position.x - initialPosition.x) / PhysicsSettings.CurrentSettings.unitsPerMeter;
            Cannon.times[testIndex] = flightTime;

            body.enabled = false;
            Destroy(gameObject, 10);
        }
    }
}
