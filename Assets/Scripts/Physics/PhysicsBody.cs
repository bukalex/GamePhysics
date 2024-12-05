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
    public Vector3 Force { get; private set; }
    public Vector3 AngularVelocity { get; set; }
    public Vector3 Torque { get; private set; }
    public Quaternion Rotation
    {
        get
        {
            return transform.rotation;
        }
        set
        {
            transform.rotation = value;
        }
    }
    public float AngularDrag
    {
        get
        {
            return angularDrag;
        }
        set
        {
            angularDrag = value > 0 ? value : 0.001f;
        }
    }
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
    public bool IsStatic = false;

    [SerializeField]
    [Min(0)]
    private float drag;
    [SerializeField]
    [Min(0.001f)]
    private float angularDrag = 1;
    [SerializeField]
    [Min(0.001f)]
    private float mass = 1;

    [SerializeField]
    private bool lockRotation = true;

    private void Awake()
    {
        PhysicsSystem.RegisterPhysicsBody(this);
    }

    private void OnDestroy()
    {
        PhysicsSystem.UnregisterPhysicsBody(this);
    }

    public void AddImpulse(Vector3 impulse)
    {
        Velocity += impulse / Mass;
    }

    public void AddForce(Vector3 force)
    {
        Force += force;
    }

    public void AddForce(Vector3 force, Vector3 position)
    {
        Vector3 distance = position - Position;

        Force += force;
        if (!lockRotation) Torque += Vector3.Cross(distance, force);
    }

    public void SetForce(Vector3 force = default)
    {
        Force = force;
    }

    public void SetTorque(Vector3 torque = default)
    {
        Torque = torque;
    }
}
