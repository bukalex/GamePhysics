using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHit
{
    public void OnHit(PhysicsShape otherShape, HitResult hitResult);
}
