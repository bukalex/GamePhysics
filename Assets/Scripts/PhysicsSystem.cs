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
            if (physicsBody.IsStatic) continue;

            ApplyGravity(physicsBody);
            ApplyTorque(physicsBody);
            ApplyAngularVelocity(physicsBody);
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
        physicsBody.AddForce(Settings.gravity * physicsBody.Mass);
    }

    private static void ApplyTorque(PhysicsBody physicsBody)
    {
        if (physicsBody.Torque.magnitude > 0)
        {
            physicsBody.AngularVelocity += (physicsBody.Torque / physicsBody.AngularDrag) * Time.fixedDeltaTime;
            physicsBody.SetTorque();
        }
        else
        {
            physicsBody.AngularVelocity +=
                -physicsBody.AngularVelocity.normalized *
                physicsBody.AngularDrag * Mathf.Pow(physicsBody.AngularVelocity.magnitude, 2) *
                Time.fixedDeltaTime;

            if (physicsBody.AngularVelocity.magnitude * Time.fixedDeltaTime < Settings.rotationThreshold / physicsBody.AngularDrag)
            {
                physicsBody.AngularVelocity = Vector3.zero;
            }
        }
    }

    private static void ApplyAngularVelocity(PhysicsBody physicsBody)
    {
        physicsBody.Rotation *= Quaternion.AngleAxis(physicsBody.AngularVelocity.magnitude * Time.fixedDeltaTime, physicsBody.AngularVelocity.normalized);
    }

    private static void ApplyForce(PhysicsBody physicsBody)
    {
        physicsBody.Velocity += (physicsBody.Force / physicsBody.Mass) * Time.fixedDeltaTime;
        physicsBody.SetForce();
    }

    private static void ApplyDamping(PhysicsBody physicsBody)
    {
        physicsBody.Velocity += 
            -physicsBody.Velocity.normalized * 
            physicsBody.Drag * Mathf.Pow(physicsBody.Velocity.magnitude, 2) * 
            Time.fixedDeltaTime;
    }

    private static void ApplyVelocity(PhysicsBody physicsBody)
    {
        if (physicsBody.Velocity.magnitude * Time.fixedDeltaTime < Settings.movementThreshold) return;

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
                        ApplyCollisionResponse(physicsShapes[shapeA], physicsShapes[shapeB], ref hitResult);
                        ApplyCollisionResponse(physicsShapes[shapeB], physicsShapes[shapeA], ref hitResult);

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
        hitResult.hitShapeA = shapeA;
        hitResult.hitShapeB = shapeB;

        if (shapeA == shapeB) return false;

        SurfacePoint pointA = shapeA.GetClosestPoint(shapeB.Position);
        SurfacePoint pointB = shapeB.GetClosestPoint(shapeA.Position);
        SurfacePoint pointA2;
        SurfacePoint pointB2;

        if (shapeA.HasFarthestPoint())
        {
            pointA2 = shapeA.GetFarthestPoint(pointB.position, -pointB.normal);
            if (shapeB.IsPointInside(pointA2.position) || shapeB.TryGetIntersectionPoint(shapeA.Position, pointA2.position, out SurfacePoint result))
            {
                if (pointA2.position == pointA.position)
                {
                    pointB2 = shapeB.GetClosestPoint(pointA.position);
                    hitResult.impactPoint = pointB2.position;
                    hitResult.impactNormal = pointB2.normal;

                    return true;
                }

                hitResult.impactPoint = pointA2.position;
                hitResult.impactNormal = pointA2.normal;

                return true;
            }
        }

        if (shapeB.HasFarthestPoint())
        {
            pointB2 = shapeB.GetFarthestPoint(pointA.position, -pointA.normal);
            if (shapeA.IsPointInside(pointB2.position) || shapeA.TryGetIntersectionPoint(shapeB.Position, pointB2.position, out SurfacePoint result))
            {
                if (pointB2.position == pointB.position)
                {
                    pointA2 = shapeA.GetClosestPoint(pointB.position);
                    hitResult.impactPoint = pointA2.position;
                    hitResult.impactNormal = pointA2.normal;

                    return true;
                }

                hitResult.impactPoint = pointB2.position;
                hitResult.impactNormal = pointB2.normal;
                
                return true;
            }
        }

        return false;
    }

    private static void ApplyCollisionResponse(PhysicsShape targetShape, PhysicsShape hitShape, ref HitResult hitResult)
    {
        if (!targetShape || !targetShape.Body || targetShape.Body.IsStatic) return;
        
        Vector3 VplaneTarget = Vector3.ProjectOnPlane(targetShape.Body.Velocity, hitResult.impactNormal);
        Vector3 VnormTarget = Vector3.Project(targetShape.Body.Velocity, hitResult.impactNormal);
        Vector3 VnormResTarget;

        #region Bounce
        float bounce = GetCoefficient(targetShape.bounce, hitShape.bounce, Settings.bounceMode);

        if (hitShape && hitShape.Body && !hitShape.Body.IsStatic)
        {
            Vector3 VplaneHit = Vector3.ProjectOnPlane(hitShape.Body.Velocity, hitResult.impactNormal);
            Vector3 VnHit = Vector3.Project(hitShape.Body.Velocity, hitResult.impactNormal);

            VnormResTarget = bounce * (2 * hitShape.Body.Mass * VnHit + VnormTarget * (targetShape.Body.Mass - hitShape.Body.Mass)) /
                (targetShape.Body.Mass + hitShape.Body.Mass);
            VplaneTarget -= VplaneHit;
        }
        else
        {
            VnormResTarget = -VnormTarget * bounce;
        }

        Vector3 Fnorm = (VnormResTarget - VnormTarget) * targetShape.Body.Mass / Time.fixedDeltaTime;
        targetShape.Body.AddForce(Fnorm, hitResult.impactPoint);
        hitResult.normalForceMagnitude = Fnorm.magnitude;
        #endregion

        #region Friction
        if (VplaneTarget.magnitude * Time.fixedDeltaTime < Settings.movementThreshold)
        {
            float dynamicFriction = GetCoefficient(targetShape.dynamicFriction, hitShape.dynamicFriction, Settings.frictionMode);

            Vector3 Fplane = Vector3.ProjectOnPlane(targetShape.Body.Force, hitResult.impactNormal);
            Vector3 Ffr = -VplaneTarget.normalized * Fnorm.magnitude * dynamicFriction;

            if (Vector3.Dot((Fplane + Ffr) / targetShape.Body.Mass * Time.fixedDeltaTime + VplaneTarget, VplaneTarget) < 0)
            {
                Ffr = targetShape.Body.Mass * (-VplaneTarget / Time.fixedDeltaTime) - Fplane;
                targetShape.Body.AddForce(Ffr);
            }
            else
            {
                targetShape.Body.AddForce(Ffr, hitResult.impactPoint);
            }
        }
        #endregion
    }

    private static void ApplyMinimumTranslation(PhysicsShape shapeA, PhysicsShape shapeB, HitResult hitResult)
    {
        bool canShapeAMove = shapeA.Body && !shapeA.Body.IsStatic;
        bool canShapeBMove = shapeB.Body && !shapeB.Body.IsStatic;

        if (!canShapeAMove && !canShapeBMove) return;
        if (!shapeA.HasFarthestPoint() && !shapeB.HasFarthestPoint()) return;

        SurfacePoint pointA = default;
        SurfacePoint pointB = default;

        if (shapeA.HasFarthestPoint())
        {
            if (shapeB.IsPointInside(shapeA.Position))
            {
                pointA = shapeA.GetFarthestPoint(hitResult.impactPoint, Vector3.Project(shapeA.Position - hitResult.impactPoint, hitResult.impactNormal));
            }
            else pointA = shapeA.GetFarthestPoint(hitResult.impactPoint, hitResult.impactNormal, true);
        }
        if (shapeB.HasFarthestPoint())
        {
            if (shapeA.IsPointInside(shapeB.Position))
            {
                pointB = shapeB.GetFarthestPoint(hitResult.impactPoint, Vector3.Project(shapeB.Position - hitResult.impactPoint, hitResult.impactNormal));
            }
            else pointB = shapeB.GetFarthestPoint(hitResult.impactPoint, hitResult.impactNormal, true);
        }

        if (!shapeA.HasFarthestPoint()) pointA = shapeA.GetClosestPoint(pointB.position);
        if (!shapeB.HasFarthestPoint()) pointB = shapeB.GetClosestPoint(pointA.position);

        Vector3 Displacement = pointB.position - pointA.position;

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
    public float normalForceMagnitude;
    public PhysicsShape hitShapeA;
    public PhysicsShape hitShapeB;
}