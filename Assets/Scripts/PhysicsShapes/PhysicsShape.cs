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
    [Min(0)]
    public float staticFriction = 0.5f;
    [Min(0)]
    public float dynamicFriction = 0.3f;
    [Range(0, 1)]
    public float bounce = 1f;

    [SerializeField]
    protected Vector3 offset;

    private IBeginOverlap[] beginOverlapListeners;
    private IOverlap[] overlapListeners;
    private IEndOverlap[] endOverlapListeners;
    private IBeginHit[] beginHitListeners;
    private IHit[] hitListeners;
    private IEndHit[] endHitListeners;

    private Dictionary<PhysicsShape, HitResult> overlapShapes = new Dictionary<PhysicsShape, HitResult>();
    private Dictionary<PhysicsShape, HitResult> hitShapes = new Dictionary<PhysicsShape, HitResult>();

    protected virtual void Awake()
    {
        Body = GetComponent<PhysicsBody>();
        PhysicsSystem.RegisterPhysicsShape(this);

        beginOverlapListeners = GetComponents<IBeginOverlap>();
        overlapListeners = GetComponents<IOverlap>();
        endOverlapListeners = GetComponents<IEndOverlap>();
        beginHitListeners = GetComponents<IBeginHit>();
        hitListeners = GetComponents<IHit>();
        endHitListeners = GetComponents<IEndHit>();
    }

    protected virtual void OnDrawGizmos()
    {
        if (overlapShapes.Count != 0 || hitShapes.Count != 0) Gizmos.color = Color.red;
        else Gizmos.color = isTrigger ? Color.green : Color.black;
        DrawWireShape();
    }

    protected virtual void OnDestroy()
    {
        PhysicsSystem.UnregisterPhysicsShape(this);
    }

    public virtual bool TryOnBeginOverlap(PhysicsShape otherShape, HitResult hitResult)
    {
        if (overlapShapes.ContainsKey(otherShape)) return false;

        OnBeginOverlap(otherShape, hitResult);
        overlapShapes.Add(otherShape, hitResult);

        return true;
    }

    protected virtual void OnBeginOverlap(PhysicsShape otherShape, HitResult hitResult)
    {
        foreach (IBeginOverlap beginOverlapListener in beginOverlapListeners)
        {
            beginOverlapListener.OnBeginOverlap(otherShape, hitResult);
        }
    }

    public virtual void OnOverlap(PhysicsShape otherShape, HitResult hitResult)
    {
        foreach (IOverlap overlapListener in overlapListeners)
        {
            overlapListener.OnOverlap(otherShape, hitResult);
        }
    }

    public virtual bool TryOnEndOverlap(PhysicsShape otherShape)
    {
        if (!overlapShapes.ContainsKey(otherShape)) return false;

        OnEndOverlap(otherShape, overlapShapes[otherShape]);
        overlapShapes.Remove(otherShape);

        return true;
    }

    protected virtual void OnEndOverlap(PhysicsShape otherShape, HitResult hitResult)
    {
        foreach (IEndOverlap endOverlapListener in endOverlapListeners)
        {
            endOverlapListener.OnEndOverlap(otherShape, hitResult);
        }
    }

    public virtual bool TryOnBeginHit(PhysicsShape otherShape, HitResult hitResult)
    {
        if (hitShapes.ContainsKey(otherShape)) return false;

        OnBeginHit(otherShape, hitResult);
        hitShapes.Add(otherShape, hitResult);

        return true;
    }

    protected virtual void OnBeginHit(PhysicsShape otherShape, HitResult hitResult)
    {
        foreach (IBeginHit beginHitListener in beginHitListeners)
        {
            beginHitListener.OnBeginHit(otherShape, hitResult);
        }
    }

    public virtual void OnHit(PhysicsShape otherShape, HitResult hitResult)
    {
        foreach (IHit hitListener in hitListeners)
        {
            hitListener.OnHit(otherShape, hitResult);
        }
    }

    public virtual bool TryOnEndHit(PhysicsShape otherShape)
    {
        if (!hitShapes.ContainsKey(otherShape)) return false;

        OnEndHit(otherShape, hitShapes[otherShape]);
        hitShapes.Remove(otherShape);

        return true;
    }

    protected virtual void OnEndHit(PhysicsShape otherShape, HitResult hitResult)
    {
        foreach (IEndHit endHitListener in endHitListeners)
        {
            endHitListener.OnEndHit(otherShape, hitResult);
        }
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