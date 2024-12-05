using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBeginOverlap
{
    public void OnBeginOverlap(PhysicsShape otherShape, HitResult hitResult);
}

public interface IOverlap
{
    public void OnOverlap(PhysicsShape otherShape, HitResult hitResult);
}

public interface IEndOverlap
{
    public void OnEndOverlap(PhysicsShape otherShape, HitResult hitResult);
}