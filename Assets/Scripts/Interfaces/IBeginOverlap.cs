using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBeginOverlap
{
    public void OnBeginOverlap(PhysicsShape otherShape, HitResult hitResult);
}
