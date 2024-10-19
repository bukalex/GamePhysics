using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parcel : MonoBehaviour, IBeginOverlap, IOverlap, IEndOverlap
{
    public void OnBeginOverlap(PhysicsShape otherShape, HitResult hitResult)
    {
        Debug.Log("BeginOverlap");
    }

    public void OnOverlap(PhysicsShape otherShape, HitResult hitResult)
    {
        Debug.Log("Overlap");
    }

    public void OnEndOverlap(PhysicsShape otherShape, HitResult hitResult)
    {
        Debug.Log("EndOverlap");
    }
}
