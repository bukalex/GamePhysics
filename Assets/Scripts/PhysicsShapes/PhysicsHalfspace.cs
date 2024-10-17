using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsHalfspace : PhysicsPlane
{
    public override bool IsPointInside(Vector3 point)
    {
        return Mathf.Round(Vector3.Dot(Normal, point - Position) * 1000f) / 1000f <= 0;
    }
}
