using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsHalfspace : PhysicsPlane
{
    public override bool IsPointInside(Vector3 point)
    {
        return Vector3.Dot(Normal, point - Position) <= 0;
    }
}
