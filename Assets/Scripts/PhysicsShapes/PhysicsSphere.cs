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

    public override SurfacePoint GetFarthestPoint(Vector3 origin, Vector3 normal, bool checkBothDirections = false)
    {
        SurfacePoint point = default;

        if (!checkBothDirections)
        {
            point.normal = normal;
            point.position = Position + point.normal * radius;
        }
        else
        {
            Vector3 posA = Position + normal * radius;
            Vector3 posB = Position - normal * radius;

            if ((posA - origin).magnitude < (posB - origin).magnitude)
            {
                point.normal = normal;
                point.position = posA;
            }
            else
            {
                point.normal = -normal;
                point.position = posB;
            }
        }

        return point;
    }

    public override bool IsOverlapingWithShape(PhysicsShape otherShape, out HitResult hitResult)
    {
        hitResult = default;
        hitResult.hitShapeA = this;
        hitResult.hitShapeB = otherShape;

        SurfacePoint pointB = otherShape.GetClosestPoint(Position);
        if (IsPointInside(pointB.position))
        {
            hitResult.impactPoint = pointB.position;
            hitResult.impactNormal = (Position - hitResult.impactPoint).normalized;
            hitResult.depth = radius - (Position - hitResult.impactPoint).magnitude;

            return true;
        }

        return false;
    }

    public override bool IsPointInside(Vector3 point)
    {
        return Vector3.Distance(Position, point) <= radius;
    }

    protected override void DrawWireShape()
    {
        Gizmos.matrix = Matrix4x4.TRS(Position, transform.rotation, Vector3.one);
        Gizmos.DrawWireSphere(Vector3.zero, radius);
    }
}
