using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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
        }

        RunCollisionChecks();

        foreach (PhysicsBody physicsBody in physicsBodies)
        {
            if (!physicsBody) continue;
            if (!physicsBody.isActiveAndEnabled) continue;

            ApplyVelocity(physicsBody);
        }
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

                        ApplyMinimumTranslation(physicsShapes[shapeB], physicsShapes[shapeA]);
                        ApplyCollisionResponse(physicsShapes[shapeA], physicsShapes[shapeB], hitResult);
                        ApplyCollisionResponse(physicsShapes[shapeB], physicsShapes[shapeA], hitResult);

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

    private static void ApplyCollisionResponse(PhysicsShape targetShape, PhysicsShape hitShape, HitResult hitResult)
    {
        if (!targetShape || !targetShape.Body || targetShape.Body.IsStatic) return;
        
        Vector3 projectionOnPlaneTarget = Vector3.ProjectOnPlane(targetShape.Body.Velocity, hitResult.impactNormal);
        Vector3 projectionOnNormalTarget = Vector3.Project(targetShape.Body.Velocity, hitResult.impactNormal);
        Vector3 pendingVelocityTarget;

        #region Friction
        float dynamicFriction = 1;
        switch (Settings.frictionMode)
        {
            case CoefficientBlendMode.Add:
                dynamicFriction = targetShape.dynamicFriction + hitShape.dynamicFriction;
                break;

            case CoefficientBlendMode.Multiply:
                dynamicFriction = targetShape.dynamicFriction * hitShape.dynamicFriction;
                break;

            case CoefficientBlendMode.Average:
                dynamicFriction = (targetShape.dynamicFriction + hitShape.dynamicFriction) / 2;
                break;
        }

        float alpha = Vector3.Angle(hitResult.impactNormal, Settings.gravity) * Mathf.Deg2Rad;
        float normalForceMagnitude = targetShape.Body.Mass * Settings.gravity.magnitude * Mathf.Abs(Mathf.Cos(alpha));
        Vector3 frictionForce = -projectionOnPlaneTarget.normalized * normalForceMagnitude * dynamicFriction;

        projectionOnPlaneTarget += frictionForce / targetShape.Body.Mass * Time.fixedDeltaTime;
        if (Vector3.Dot(projectionOnPlaneTarget, frictionForce) > 0) projectionOnPlaneTarget = Vector3.zero;

        Debug.DrawLine(targetShape.Position, targetShape.Position - projectionOnNormalTarget.normalized * normalForceMagnitude, Color.green);
        Debug.DrawLine(targetShape.Position, targetShape.Position + frictionForce, Color.yellow);
        Debug.DrawLine(targetShape.Position, targetShape.Position + targetShape.Body.Mass * Settings.gravity, Color.magenta);
        #endregion

        #region Bounce
        float bounce = 1;
        switch (Settings.bounceMode)
        {
            case CoefficientBlendMode.Add:
                bounce = targetShape.bounce + hitShape.bounce;
                break;

            case CoefficientBlendMode.Multiply:
                bounce = targetShape.bounce * hitShape.bounce;
                break;

            case CoefficientBlendMode.Average:
                bounce = (targetShape.bounce + hitShape.bounce) / 2;
                break;
        }

        if (hitShape && hitShape.Body && !hitShape.Body.IsStatic)
        {
            Vector3 projectionOnNormalHit = Vector3.Project(hitShape.Body.Velocity, hitResult.impactNormal);
            pendingVelocityTarget = projectionOnPlaneTarget + bounce *
                (2 * hitShape.Body.Mass * projectionOnNormalHit + projectionOnNormalTarget * (targetShape.Body.Mass - hitShape.Body.Mass)) /
                (targetShape.Body.Mass + hitShape.Body.Mass);
        }
        else
        {
            pendingVelocityTarget = projectionOnPlaneTarget - projectionOnNormalTarget * bounce;
        }
        #endregion

        targetShape.Body.SaveVelocity(pendingVelocityTarget);
    }

    private static void ApplyMinimumTranslation(PhysicsShape shapeA, PhysicsShape shapeB)
    {
        bool canShapeAMove = shapeA.Body && !shapeA.Body.IsStatic;
        bool canShapeBMove = shapeB.Body && !shapeB.Body.IsStatic;

        if (!canShapeAMove && !canShapeBMove) return;

        SurfacePoint pointA = shapeA.GetClosestPoint(shapeB.Position);
        SurfacePoint pointB = shapeB.GetClosestPoint(shapeA.Position);
        
        Vector3 AB = pointB.position - pointA.position;
        Vector3 Displacement = AB;

        float ABAngle = Vector3.Angle(pointA.normal, pointB.normal);
        float maxAngle = 5;

        if (maxAngle < ABAngle && ABAngle < (180 - maxAngle))
        {
            SurfacePoint pointA2 = shapeA.GetClosestPoint(pointB.position);
            SurfacePoint pointB2 = shapeB.GetClosestPoint(pointA.position);

            float A2BAngle = Vector3.Angle(pointA2.normal, pointB.normal);
            float AB2Angle = Vector3.Angle(pointA.normal, pointB2.normal);

            if (A2BAngle <= maxAngle || (180 - maxAngle) <= A2BAngle)
            {
                Displacement = pointB.position - pointA2.position;
            }
            else if (AB2Angle <= maxAngle || (180 - maxAngle) <= AB2Angle)
            {
                Displacement = pointB2.position - pointA.position;
            }
        }

        if (canShapeAMove) shapeA.Body.Position += Displacement * (canShapeBMove ? 0.5f : 1);
        if (canShapeBMove) shapeB.Body.Position += -Displacement * (canShapeAMove ? 0.5f : 1);
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