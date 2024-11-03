using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhysicsSystem : MonoBehaviour
{
    private static List<PhysicsBody> physicsBodies = new List<PhysicsBody>();
    private static List<PhysicsShape> physicsShapes = new List<PhysicsShape>();
    private static GameObject instance = null;

    private static PhysicsSettings Settings
    {
        get
        {
            return PhysicsSettings.CurrentSettings;
        }
    }

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            Debug.LogWarning("Physics System instance already exists.");

            return;
        }

        instance = gameObject;
        SceneManager.sceneUnloaded += OnSceneUnload;
    }

    private void FixedUpdate()
    {
        if (!Settings)
        {
            Debug.LogWarning("No physics settings enabled.");
            return;
        }

        foreach (PhysicsBody physicsBody in physicsBodies)
        {
            if (!physicsBody) continue;
            if (!physicsBody.isActiveAndEnabled) continue;

            ApplyGravity(physicsBody);
            ApplyDamping(physicsBody);
            ApplyVelocity(physicsBody);
        }

        RunCollisionChecks();
    }

    #region Registration
    private void OnSceneUnload(Scene scene)
    {
        physicsBodies.Clear();
        PrintLog("Physics body list was cleared.");

        physicsShapes.Clear();
        PrintLog("Physics shape list was cleared.");

        SceneManager.sceneUnloaded -= OnSceneUnload;
    }

    private static void InstantiatePhysicsSystem()
    {
        GameObject newInstance = new GameObject("Physics System");
        newInstance.AddComponent<PhysicsSystem>();
        PrintLog("Physics System instance was created.");
    } 

    public static void RegisterPhysicsBody(PhysicsBody physicsBody)
    {
        if (!instance) InstantiatePhysicsSystem();
        if (physicsBodies.Contains(physicsBody)) return;

        physicsBodies.Add(physicsBody);
        PrintLog("Physics body was registred. Physics bodies registred in simulation: " + physicsBodies.Count);
    }

    public static void UnregisterPhysicsBody(PhysicsBody physicsBody)
    {
        physicsBodies.Remove(physicsBody);
        PrintLog("Physics body was unregistred. Physics bodies registred in simulation: " + physicsBodies.Count);
    }

    public static void RegisterPhysicsShape(PhysicsShape physicsShape)
    {
        if (!instance) InstantiatePhysicsSystem();
        if (physicsShapes.Contains(physicsShape)) return;

        physicsShapes.Add(physicsShape);
        PrintLog("Physics shape was registred. Physics shapes registred in simulation: " + physicsShapes.Count);
    }

    public static void UnregisterPhysicsShape(PhysicsShape physicsShape)
    {
        physicsShapes.Remove(physicsShape);
        PrintLog("Physics shape was unregistred. Physics shapes registred in simulation: " + physicsShapes.Count);
    }
    #endregion

    private static void ApplyGravity(PhysicsBody physicsBody)
    {
        if (physicsBody.IsStatic) return;

        physicsBody.Velocity += Settings.gravity * Time.fixedDeltaTime;
    }

    private static void ApplyDamping(PhysicsBody physicsBody)
    {
        if (physicsBody.IsStatic) return;

        physicsBody.Velocity += 
            -physicsBody.Velocity.normalized * 
            physicsBody.Drag * Mathf.Pow(physicsBody.Velocity.magnitude, 2) * 
            Time.fixedDeltaTime;
    }

    private static void ApplyVelocity(PhysicsBody physicsBody)
    {
        if (physicsBody.IsStatic) return;

        physicsBody.Position += physicsBody.Velocity * Time.fixedDeltaTime;
        if (physicsBody.Position.y <= Settings.deadZone) Destroy(physicsBody.gameObject);
    }

    private static void RunCollisionChecks()
    {
        for (int shapeA = 0; shapeA < physicsShapes.Count - 1; shapeA++)
        {
            if (!physicsShapes[shapeA]) continue;
            if (!physicsShapes[shapeA].isActiveAndEnabled) continue;

            for (int shapeB = shapeA + 1; shapeB < physicsShapes.Count; shapeB++)
            {
                if (!physicsShapes[shapeB]) continue;
                if (!physicsShapes[shapeB].isActiveAndEnabled) continue;

                if (AreShapesOvelapping(physicsShapes[shapeA], physicsShapes[shapeB], out HitResult hitResult))
                {
                    if (physicsShapes[shapeA].isTrigger || physicsShapes[shapeB].isTrigger)
                    {
                        PrintLog("Overlapping detected: " + physicsShapes[shapeA].name + " and " + physicsShapes[shapeB].name);

                        if (!physicsShapes[shapeA].TryOnBeginOverlap(physicsShapes[shapeB], hitResult) || Settings.callOverlapAtFirstFrame)
                        {
                            physicsShapes[shapeA].OnOverlap(physicsShapes[shapeB], hitResult);
                        }
                        if (!physicsShapes[shapeB].TryOnBeginOverlap(physicsShapes[shapeA], hitResult) || Settings.callOverlapAtFirstFrame)
                        {
                            physicsShapes[shapeB].OnOverlap(physicsShapes[shapeA], hitResult);
                        }
                    }
                    else
                    {
                        PrintLog("Collision detected: " + physicsShapes[shapeA].name + " and " + physicsShapes[shapeB].name);

                        ApplyMinimumTranslation(physicsShapes[shapeB], physicsShapes[shapeA], hitResult);
                        ApplyCollisionResponse(physicsShapes[shapeA].Body, physicsShapes[shapeB].Body, hitResult);
                        ApplyCollisionResponse(physicsShapes[shapeB].Body, physicsShapes[shapeA].Body, hitResult);

                        if (physicsShapes[shapeA].Body) physicsShapes[shapeA].Body.ApplyPendingVelocity();
                        if (physicsShapes[shapeB].Body) physicsShapes[shapeB].Body.ApplyPendingVelocity();

                        if (!physicsShapes[shapeA].TryOnBeginHit(physicsShapes[shapeB], hitResult) || Settings.callHitAtFirstFrame)
                        {
                            physicsShapes[shapeA].OnHit(physicsShapes[shapeB], hitResult);
                        }
                        if (!physicsShapes[shapeB].TryOnBeginHit(physicsShapes[shapeA], hitResult) || Settings.callHitAtFirstFrame)
                        {
                            physicsShapes[shapeB].OnHit(physicsShapes[shapeA], hitResult);
                        }
                    }
                }
                else
                {
                    if (physicsShapes[shapeA].isTrigger || physicsShapes[shapeB].isTrigger)
                    {
                        physicsShapes[shapeA].TryOnEndOverlap(physicsShapes[shapeB]);
                        physicsShapes[shapeB].TryOnEndOverlap(physicsShapes[shapeA]);
                    }
                    else
                    {
                        physicsShapes[shapeA].TryOnEndHit(physicsShapes[shapeB]);
                        physicsShapes[shapeB].TryOnEndHit(physicsShapes[shapeA]);
                    }
                }
            }
        }
    }

    private static bool AreShapesOvelapping(PhysicsShape shapeA, PhysicsShape shapeB, out HitResult hitResult)
    {
        hitResult = default;

        if (shapeA == shapeB) return false;

        SurfacePoint pointA = shapeA.GetClosestPoint(shapeB.Position);
        SurfacePoint pointB = shapeB.GetClosestPoint(shapeA.Position);

        if (shapeA.IsPointInside(pointB.position))
        {
            hitResult.impactPoint = pointB.position;
            hitResult.impactNormal = shapeA.GetClosestPoint(pointB.position).normal;
            
            return true;
        }
        
        if (shapeB.IsPointInside(pointA.position))
        {
            hitResult.impactPoint = pointA.position;
            hitResult.impactNormal = shapeB.GetClosestPoint(pointA.position).normal;
            
            return true;
        }

        return false;
    }

    private static void ApplyCollisionResponse(PhysicsBody targetBody, PhysicsBody hitBody, HitResult hitResult)
    {
        if (!targetBody || targetBody.IsStatic) return;

        Vector3 projectionOnPlaneTarget = Vector3.ProjectOnPlane(targetBody.Velocity, hitResult.impactNormal);
        Vector3 projectionOnNormalTarget = Vector3.Project(targetBody.Velocity, hitResult.impactNormal);
        Vector3 pendingVelocityTarget;

        if (hitBody && !hitBody.IsStatic)
        {
            Vector3 projectionOnNormalHit = Vector3.Project(hitBody.Velocity, hitResult.impactNormal);
            pendingVelocityTarget = projectionOnPlaneTarget +
                (2 * hitBody.Mass * projectionOnNormalHit + projectionOnNormalTarget * (targetBody.Mass - hitBody.Mass)) /
                (targetBody.Mass + hitBody.Mass);
        }
        else
        {
            pendingVelocityTarget = projectionOnPlaneTarget - projectionOnNormalTarget;
        }

        targetBody.SaveVelocity(pendingVelocityTarget);
    }

    private static void ApplyMinimumTranslation(PhysicsShape shapeA, PhysicsShape shapeB, HitResult hitResult)
    {
        bool canShapeAMove = shapeA.Body && !shapeA.Body.IsStatic;
        bool canShapeBMove = shapeB.Body && !shapeB.Body.IsStatic;

        if (!canShapeAMove && !canShapeBMove) return;

        Vector3 pointA = shapeA.GetClosestPoint(hitResult.impactPoint).position;
        Vector3 pointB = shapeB.GetClosestPoint(hitResult.impactPoint).position;

        if (shapeA.IsPointInside(shapeB.Position))
        {
            pointB = shapeB.GetOppositePoint(pointB, hitResult.impactNormal);
        }
        else if (shapeB.IsPointInside(shapeA.Position))
        {
            pointA = shapeA.GetOppositePoint(pointA, hitResult.impactNormal);
        }

        if (canShapeAMove) shapeA.Body.Position += (pointB - pointA) * (canShapeBMove ? 0.5f : 1);
        if (canShapeBMove) shapeB.Body.Position += (pointA - pointB) * (canShapeAMove ? 0.5f : 1);
    }

    private static void PrintLog(string log)
    {
        if (!Settings) Debug.LogWarning("No physics settings enabled.");
        else if (Settings.enableLogs) Debug.Log(log);
    }
}

public struct HitResult
{
    public Vector3 impactPoint;
    public Vector3 impactNormal;
}