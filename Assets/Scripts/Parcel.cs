using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parcel : MonoBehaviour, IHit
{
    public void OnHit(PhysicsShape otherShape, HitResult hitResult)
    {
        Destroy(gameObject);
    }
}
