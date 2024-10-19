using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBeginHit
{
    public void OnBeginHit(PhysicsShape otherShape, HitResult hitResult);
}

public interface IHit
{
    public void OnHit(PhysicsShape otherShape, HitResult hitResult);
}

public interface IEndHit
{
    public void OnEndHit(PhysicsShape otherShape, HitResult hitResult);
}