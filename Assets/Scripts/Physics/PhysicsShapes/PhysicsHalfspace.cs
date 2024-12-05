using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsHalfspace : PhysicsPlane
{
    public override bool IsOverlapingWithShape(PhysicsShape otherShape, out HitResult hitResult)
    {
        hitResult = default;
        hitResult.hitShapeA = this;
        hitResult.hitShapeB = otherShape;

        if (otherShape.HasFarthestPoint())
        {
            SurfacePoint pointB = otherShape.GetFarthestPoint(Position, -Normal);
            if (IsPointInside(pointB.position))
            {
                hitResult.impactPoint = GetClosestPoint(pointB.position).position;
                hitResult.impactNormal = Normal;
                hitResult.depth = (pointB.position - hitResult.impactPoint).magnitude;

                return true;
            }
        }

        return false;
    }

    public override bool IsPointInside(Vector3 point)
    {
        return Mathf.Round(Vector3.Dot(Normal, point - Position) * 1000f) / 1000f <= 0;
    }
}
