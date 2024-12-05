using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToughnessComponent : MonoBehaviour, IBeginHit
{
    [SerializeField]
    private float toughness = 500;

    [SerializeField]
    private int scorePoints = 1;

    [SerializeField]
    [Range(0, 1)]
    private float energySavedOnDestroy = 0.5f;

    private ScoreCounter scoreCounter;

    private void Awake()
    {
        scoreCounter = FindObjectOfType<ScoreCounter>();
    }

    public void OnBeginHit(PhysicsShape otherShape, HitResult hitResult)
    {
        if (hitResult.normalForceMagnitude >= toughness)
        {
            if (otherShape && otherShape.Body)
            {
                otherShape.Body.AddForce(Vector3.Project(hitResult.impactPoint - otherShape.Position, hitResult.impactNormal).normalized * hitResult.normalForceMagnitude * energySavedOnDestroy);
            }

            if (scoreCounter) scoreCounter.ChangeScore(scorePoints);
            Destroy(gameObject);
        }
    }
}
