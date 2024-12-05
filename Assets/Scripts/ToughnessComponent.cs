using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToughnessComponent : MonoBehaviour, IBeginHit
{
    [SerializeField]
    private float toughness = 500;

    public void OnBeginHit(PhysicsShape otherShape, HitResult hitResult)
    {
        if (hitResult.normalForceMagnitude >= toughness) Destroy(gameObject);
    }
}
