using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class PhysicsCube : PhysicsShape
{
    [SerializeField]
    protected Vector3 size = Vector3.one;

    public override SurfacePoint GetClosestPoint(Vector3 otherPoint)
    {
        SurfacePoint point = default;

        Vector3 localPosition = Quaternion.Inverse(transform.rotation) * (otherPoint - Position);

        if (IsPointInside(otherPoint))
        {
            Vector3 dx = ((localPosition.x >= 0 ? 1 : -1) * size.x - localPosition.x) * Vector3.right;
            Vector3 dy = ((localPosition.y >= 0 ? 1 : -1) * size.y - localPosition.y) * Vector3.up;
            Vector3 dz = ((localPosition.z >= 0 ? 1 : -1) * size.z - localPosition.z) * Vector3.forward;

            if (dx.magnitude < dy.magnitude)
            {
                if (dx.magnitude < dz.magnitude) localPosition += dx;
                else localPosition += dz;
            }
            else
            {
                if (dy.magnitude < dz.magnitude) localPosition += dy;
                else localPosition += dz;
            }
        }
        else
        {
            localPosition = new Vector3(
                Mathf.Clamp(localPosition.x, -size.x, size.x),
                Mathf.Clamp(localPosition.y, -size.y, size.y),
                Mathf.Clamp(localPosition.z, -size.z, size.z));
        }

        point.position = Position + transform.rotation * localPosition;
        point.normal = (otherPoint - point.position).normalized;

        return point;
    }

    public override SurfacePoint GetFarthestPoint(Vector3 origin, Vector3 normal, bool checkBothDirections = false)
    {
        SurfacePoint point = default;

        if (!checkBothDirections)
        {
            point = GetClosestPoint(origin + normal * 1000);
        }
        else
        {
            SurfacePoint pointA = GetClosestPoint(origin + normal * 1000);
            SurfacePoint pointB = GetClosestPoint(origin - normal * 1000);

            if ((pointA.position - origin).magnitude < (pointB.position - origin).magnitude) point = pointA;
            else point = pointB;
        }
        point.normal = normal;

        return point;
    }

    public override bool IsOverlapingWithShape(PhysicsShape otherShape, out HitResult hitResult)
    {
        hitResult = default;
        hitResult.hitShapeA = this;
        hitResult.hitShapeB = otherShape;

        if (otherShape.HasFarthestPoint())
        {
            SurfacePoint pointA = GetClosestPoint(otherShape.Position);
            if (otherShape.IsPointInside(pointA.position))
            {
                SurfacePoint pointB = otherShape.GetClosestPoint(pointA.position);
                hitResult.impactPoint = pointA.position;
                hitResult.impactNormal = pointB.normal;
                hitResult.depth = (pointA.position - pointB.position).magnitude;

                return true;
            }
        }
        else
        {
            SurfacePoint pointB = otherShape.GetClosestPoint(Position);
            if (IsPointInside(pointB.position))
            {
                SurfacePoint pointA = GetClosestPoint(pointB.position);
                hitResult.impactPoint = pointB.position;
                hitResult.impactNormal = pointB.normal;
                hitResult.depth = (pointA.position - pointB.position).magnitude;

                return true;
            }
        }

        return false;
    }

    public override bool IsPointInside(Vector3 point)
    {
        Vector3 localPosition = Quaternion.Inverse(transform.rotation) * (point - Position);

        return 
            Mathf.Abs(localPosition.x) <= size.x && 
            Mathf.Abs(localPosition.y) <= size.y && 
            Mathf.Abs(localPosition.z) <= size.z;
    }

    protected override void DrawWireShape()
    {
        Gizmos.matrix = Matrix4x4.TRS(Position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, size * 2);
    }
}
