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
        return Vector3.Dot(Normal, point - Position) == 0;
    }

    protected override void DrawWireShape()
    {
        Gizmos.DrawLine(Position, Position + Normal);
        Gizmos.matrix = Matrix4x4.TRS(Position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(2, 0, 2));
    }
}
