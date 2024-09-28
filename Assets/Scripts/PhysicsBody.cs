using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBody : MonoBehaviour
{
    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
        set
        {
            transform.position = value;
        }
    }
    public Vector3 Velocity { get; set; }
    public float Drag
    {
        get
        {
            return drag;
        }
        set
        {
            drag = value > 0 ? value : 0;
        }
    }
    public float Mass
    {
        get
        {
            return mass;
        }
        set
        {
            mass = value > 0 ? value : 0.001f;
        }
    }

    [SerializeField]
    [Min(0)]
    private float drag;
    [SerializeField]
    [Min(0.001f)]
    private float mass;

    private void Awake()
    {
        PhysicsSystem.RegisterPhysicsBody(this);
    }

    private void OnDestroy()
    {
        PhysicsSystem.UnregisterPhysicsBody(this);
    }
}
