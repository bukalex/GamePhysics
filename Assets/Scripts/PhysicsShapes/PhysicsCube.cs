using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCube : PhysicsShape
{
    [SerializeField]
    protected Vector3 size = Vector3.one;

    public override SurfacePoint GetClosestPoint(Vector3 otherPoint)
    {
        SurfacePoint point = default;

        Vector3 localPosition = otherPoint - Position;
        point.position = Position + new Vector3(
            Mathf.Clamp(localPosition.x, -size.x, size.x), 
            Mathf.Clamp(localPosition.y, -size.y, size.y), 
            Mathf.Clamp(localPosition.z, -size.z, size.z));
        point.normal = (localPosition - point.position).normalized;

        return point;
    }

    public override bool IsPointInside(Vector3 point)
    {
        Vector3 localPosition = point - Position;

        return 
            Mathf.Abs(localPosition.x) <= size.x && 
            Mathf.Abs(localPosition.y) <= size.y && 
            Mathf.Abs(localPosition.z) <= size.z;
    }
}
