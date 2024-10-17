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
    [HideInInspector]
    public bool isOverlapping = false;

    [SerializeField]
    protected Vector3 offset;
    protected Coroutine CurrentDrawCollisionCoroutine;

    protected IBeginOverlap[] beginOverlapListeners;
    protected IHit[] hitListeners;

    protected virtual void Awake()
    {
        Body = GetComponent<PhysicsBody>();
        PhysicsSystem.RegisterPhysicsShape(this);

        beginOverlapListeners = GetComponents<IBeginOverlap>();
        hitListeners = GetComponents<IHit>();
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

    public virtual void OnBeginOverlap(PhysicsShape otherShape, HitResult hitResult)
    {
        foreach (IBeginOverlap beginOverlapListener in beginOverlapListeners)
        {
            beginOverlapListener.OnBeginOverlap(otherShape, hitResult);
        }
    }

    public virtual void OnHit(PhysicsShape otherShape, HitResult hitResult)
    {
        foreach (IHit hitListener in hitListeners)
        {
            hitListener.OnHit(otherShape, hitResult);
        }
    }

    public virtual void DrawCollision(float duration)
    {
        if (CurrentDrawCollisionCoroutine != null) StopCoroutine(CurrentDrawCollisionCoroutine);
        CurrentDrawCollisionCoroutine = StartCoroutine(DrawCollisionCoroutine(duration));
    }

    protected virtual IEnumerator DrawCollisionCoroutine(float duration)
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