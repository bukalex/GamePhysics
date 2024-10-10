using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicsShape : MonoBehaviour
{
    public PhysicsBody Body { get; private set; }
    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    public bool isTrigger = false;

    protected virtual void Awake()
    {
        Body = GetComponent<PhysicsBody>();
        PhysicsSystem.RegisterPhysicsShape(this);
    }

    protected virtual void OnDestroy()
    {
        PhysicsSystem.UnregisterPhysicsShape(this);
    }

    public abstract SurfacePoint GetClosestPoint(Vector3 otherPoint);
    public abstract bool IsPointInside(Vector3 point);
}

public struct SurfacePoint
{
    public Vector3 position;
    public Vector3 normal;
}