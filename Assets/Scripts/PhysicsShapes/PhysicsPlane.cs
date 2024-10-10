using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsPlane : PhysicsShape
{
    public override SurfacePoint GetClosestPoint(Vector3 otherPoint)
    {
        SurfacePoint point = default;

        point.normal = transform.up;
        point.position = Position + Vector3.ProjectOnPlane(otherPoint - Position, transform.up);

        return point;
    }

    public override bool IsPointInside(Vector3 point)
    {
        return false;
    }
}
