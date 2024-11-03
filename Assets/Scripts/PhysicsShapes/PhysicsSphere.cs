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

    public override Vector3 GetOppositePoint(Vector3 otherPoint, Vector3 direction = default)
    {
        if (direction == Vector3.zero) return otherPoint + (Position - otherPoint) * 2;
        else
        {
            Vector3 vector = otherPoint - Position;
            float alpha = Vector3.Angle(vector, direction);

            return Position + Quaternion.AngleAxis(180 - alpha * 2, Vector3.Cross(direction, vector)) * vector;
        }
    }

    protected override void DrawWireShape()
    {
        Gizmos.DrawWireSphere(Position, radius);
    }
}
