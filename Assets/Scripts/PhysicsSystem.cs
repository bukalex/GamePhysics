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
            ApplyForce(physicsBody);
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

        physicsBody.Force += Settings.gravity * physicsBody.Mass;
    }

    private static void ApplyDamping(PhysicsBody physicsBody)
    {
        if (physicsBody.IsStatic) return;

        physicsBody.Velocity += 
            -physicsBody.Velocity.normalized * 
            physicsBody.Drag * Mathf.Pow(physicsBody.Velocity.magnitude, 2) * 
            Time.fixedDeltaTime;
    }

    private static void ApplyForce(PhysicsBody physicsBody)
    {
        if (physicsBody.IsStatic) return;

        physicsBody.Velocity += (physicsBody.Force / physicsBody.Mass) * Time.fixedDeltaTime;
        physicsBody.Force = Vector3.zero;
    }

    private static void ApplyVelocity(PhysicsBody physicsBody)
    {
        if (physicsBody.IsStatic) return;
        if (physicsBody.Velocity.magnitude < Settings.velocityThreshold) return;

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
        
        Vector3 VplaneTarget = Vector3.ProjectOnPlane(targetShape.Body.Velocity, hitResult.impactNormal);
        Vector3 VnormTarget = Vector3.Project(targetShape.Body.Velocity, hitResult.impactNormal);
        Vector3 VnormResTarget;

        #region Bounce
        float bounce = GetCoefficient(targetShape.bounce, hitShape.bounce, Settings.bounceMode);

        if (hitShape && hitShape.Body && !hitShape.Body.IsStatic)
        {
            Vector3 VnHit = Vector3.Project(hitShape.Body.Velocity, hitResult.impactNormal);
            VnormResTarget = bounce * (2 * hitShape.Body.Mass * VnHit + VnormTarget * (targetShape.Body.Mass - hitShape.Body.Mass)) /
                (targetShape.Body.Mass + hitShape.Body.Mass);
        }
        else
        {
            VnormResTarget = -VnormTarget * bounce;
        }

        Vector3 Fnorm = (VnormResTarget - VnormTarget) * targetShape.Body.Mass / Time.fixedDeltaTime;
        targetShape.Body.Force += Fnorm;
        #endregion

        #region Friction
        float dynamicFriction = GetCoefficient(targetShape.dynamicFriction, hitShape.dynamicFriction, Settings.frictionMode);

        Vector3 Fplane = Vector3.ProjectOnPlane(targetShape.Body.Force, hitResult.impactNormal);
        Vector3 Ffr = -VplaneTarget.normalized * Fnorm.magnitude * dynamicFriction;

        if (Vector3.Dot((Fplane + Ffr) / targetShape.Body.Mass * Time.fixedDeltaTime + VplaneTarget, VplaneTarget) < 0)
        {
            Ffr = targetShape.Body.Mass * (-VplaneTarget / Time.fixedDeltaTime);
        }

        targetShape.Body.Force += Ffr;
        #endregion
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

        float maxAngle = 5;

        if (maxAngle < Vector3.Angle(AB, pointB.normal) && Vector3.Angle(AB, pointB.normal) < (180 - maxAngle) ||
            maxAngle < Vector3.Angle(pointA.normal, AB) && Vector3.Angle(pointA.normal, AB) < (180 - maxAngle))
        {
            SurfacePoint pointA2 = shapeA.GetClosestPoint(pointB.position);
            SurfacePoint pointB2 = shapeB.GetClosestPoint(pointA.position);

            Vector3 A2B = pointB.position - pointA2.position;
            Vector3 AB2 = pointB2.position - pointA.position;
            Vector3 A2B2 = pointB2.position - pointA2.position;

            if ((Vector3.Angle(A2B, pointB.normal) <= maxAngle || (180 - maxAngle) <= Vector3.Angle(A2B, pointB.normal)) &&
                (Vector3.Angle(pointA2.normal, A2B) <= maxAngle || (180 - maxAngle) <= Vector3.Angle(pointA2.normal, A2B)))
            {
                Displacement = A2B;
            }
            else if ((Vector3.Angle(AB2, pointB2.normal) <= maxAngle || (180 - maxAngle) <= Vector3.Angle(AB2, pointB2.normal)) &&
                     (Vector3.Angle(pointA.normal, AB2) <= maxAngle || (180 - maxAngle) <= Vector3.Angle(pointA.normal, AB2)))
            {
                Displacement = AB2;
            }
            else if ((Vector3.Angle(A2B2, pointB2.normal) <= maxAngle || (180 - maxAngle) <= Vector3.Angle(A2B2, pointB2.normal)) &&
                     (Vector3.Angle(pointA2.normal, A2B2) <= maxAngle || (180 - maxAngle) <= Vector3.Angle(pointA2.normal, A2B2)))
            {
                Displacement = A2B2;
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

    private static float GetCoefficient(float factor1, float factor2, CoefficientBlendMode mode)
    {
        switch (mode)
        {
            case CoefficientBlendMode.Add:
                return factor1 + factor2;

            case CoefficientBlendMode.Multiply:
                return factor1 * factor2;

            case CoefficientBlendMode.Average:
                return (factor1 + factor2) / 2;

            default:
                return 1;
        }
    }
}

public struct HitResult
{
    public Vector3 impactPoint;
    public Vector3 impactNormal;
}