using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    [SerializeField]
    private float minimumDisplacement = 0.1f;

    [SerializeField]
    private float destroyDelay = 3f;

    private PhysicsBody body;
    private float destroyTimer;

    void Awake()
    {
        body = GetComponent<PhysicsBody>();
        body.IsStatic = true;
    }

    private void Update()
    {
        if (body.IsStatic) return;

        if (body.Velocity.magnitude * Time.fixedDeltaTime < minimumDisplacement)
        {
            destroyTimer += Time.deltaTime;
            if (destroyTimer >= destroyDelay) Destroy(gameObject);
        }
        else
        {
            destroyTimer = 0;
        }
    }

    public void Launch(Vector3 impulse)
    {
        body.AddImpulse(impulse);
        body.IsStatic = false;
    }

    public float GetMass()
    {
        return body.Mass;
    }
}
