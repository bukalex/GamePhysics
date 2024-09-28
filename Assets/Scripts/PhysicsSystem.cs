using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhysicsSystem : MonoBehaviour
{
    private static List<PhysicsBody> physicsBodies = new List<PhysicsBody>();
    private static GameObject instance = null;

    private static Vector3 gravity = new Vector3(0, -9.8f, 0);

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
        if (physicsBodies.Count == 0) return;

        foreach (PhysicsBody physicsBody in physicsBodies)
        {
            if (!physicsBody) continue;

            ApplyGravity(physicsBody);
            ApplyDamping(physicsBody);
            ApplyVelocity(physicsBody);
        }
    }

    private void OnSceneUnload(Scene scene)
    {
        physicsBodies.Clear();
        Debug.Log("Physics body list was cleared.");

        SceneManager.sceneUnloaded -= OnSceneUnload;
    }

    private static void InstantiatePhysicsSystem()
    {
        GameObject newInstance = new GameObject("Physics System");
        newInstance.AddComponent<PhysicsSystem>();
        Debug.Log("Physics System instance was created.");
    } 

    public static void RegisterPhysicsBody(PhysicsBody physicsBody)
    {
        if (!instance) InstantiatePhysicsSystem();
        if (physicsBodies.Contains(physicsBody)) return;

        physicsBodies.Add(physicsBody);
        Debug.Log("Physics body was registred. Physics bodies registred in simulation: " + physicsBodies.Count);
    }

    public static void UnregisterPhysicsBody(PhysicsBody physicsBody)
    {
        physicsBodies.Remove(physicsBody);
        Debug.Log("Physics body was unregistred. Physics bodies registred in simulation: " + physicsBodies.Count);
    }

    private static void ApplyGravity(PhysicsBody physicsBody)
    {
        physicsBody.Velocity += gravity * Time.fixedDeltaTime;
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
        physicsBody.Position += physicsBody.Velocity * Time.fixedDeltaTime;
    }
}
