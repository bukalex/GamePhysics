using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsSphere : PhysicsShape
{
    [SerializeField]
    [Min(0)]
    protected float radius = 1;

    public override SurfacePoint GetClosestPoint(Vector3 otherPoint)
    {
        SurfacePoint point = default;

        point.normal = (otherPoint - Position).normalized;
        point.position = Position + point.normal * radius;

        return point;
    }

    public override bool IsPointInside(Vector3 point)
    {
        return Vector3.Distance(Position, point) <= radius;
    }
}
