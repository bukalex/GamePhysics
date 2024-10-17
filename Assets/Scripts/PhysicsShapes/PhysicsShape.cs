using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicsShape : MonoBehaviour
{
    public PhysicsBody Body { get; private set; }
    public Vector3 Position
    {
        get
        {
            return transform.position + offset;
        }
    }

    public bool isTrigger = false;

    [SerializeField]
    protected Vector3 offset;
    public bool isOverlapping = false;
    protected Coroutine CurrentDrawCollisionCoroutine;

    protected virtual void Awake()
    {
        Body = GetComponent<PhysicsBody>();
        PhysicsSystem.RegisterPhysicsShape(this);
    }

    protected virtual void OnDrawGizmos()
    {
        if (isOverlapping) Gizmos.color = Color.red;
        else Gizmos.color = isTrigger ? Color.green : Color.black;
        DrawWireShape();
    }

    protected virtual void OnDestroy()
    {
        PhysicsSystem.UnregisterPhysicsShape(this);
    }

    public virtual void DrawCollision(float duration)
    {
        if (CurrentDrawCollisionCoroutine != null) StopCoroutine(CurrentDrawCollisionCoroutine);
        CurrentDrawCollisionCoroutine = StartCoroutine(DrawCollisionCoroutine(duration));
    }

    protected IEnumerator DrawCollisionCoroutine(float duration)
    {
        isOverlapping = true;
        yield return new WaitForSeconds(duration);
        isOverlapping = false;
    }

    public abstract SurfacePoint GetClosestPoint(Vector3 otherPoint);
    public abstract bool IsPointInside(Vector3 point);
    protected abstract void DrawWireShape();
}

public struct SurfacePoint
{
    public Vector3 position;
    public Vector3 normal;
}