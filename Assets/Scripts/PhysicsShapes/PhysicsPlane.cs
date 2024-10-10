using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsPlane : PhysicsShape
{
    protected Vector3 Normal
    {
        get
        {
            return transform.up;
        }
    }

    public override SurfacePoint GetClosestPoint(Vector3 otherPoint)
    {
        SurfacePoint point = default;

        point.normal = Normal;
        point.position = Position + Vector3.ProjectOnPlane(otherPoint - Position, Normal);

        return point;
    }

    public override bool IsPointInside(Vector3 point)
    {
        return false;
    }
}
