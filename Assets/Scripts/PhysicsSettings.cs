using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PhysicsSettings", menuName = "Physics Settings")]
public class PhysicsSettings : ScriptableObject
{
    public static PhysicsSettings CurrentSettings
    {
        get
        {
            if (allSettings == null) FindSettingsAssets();

            if (allSettings != null)
            {
                foreach (PhysicsSettings settings in allSettings)
                {
                    if (settings.isEnabled)
                    {
                        currentSettings = settings;
                        break;
                    }
                }
            }

            return currentSettings;
        }
        set
        {
            currentSettings = value;
        }
    }
    public static PhysicsSettings currentSettings;

    private static string path = "Settings";
    private static PhysicsSettings[] allSettings;

    [SerializeField]
    private bool isEnabled = false;

    public Vector3 gravity = new Vector3(0, -9.8f, 0);
    public float deadZone = -25;
    public float movementThreshold = 0.001f;
    public float rotationThreshold = 0.05f;
    public bool enableLogs = false;
    public bool callOverlapAtFirstFrame = false;
    public bool callHitAtFirstFrame = false;
    public CoefficientBlendMode frictionMode = CoefficientBlendMode.Average;
    public CoefficientBlendMode bounceMode = CoefficientBlendMode.Average;

    private static void FindSettingsAssets()
    {
        allSettings = Resources.LoadAll<PhysicsSettings>(path);
    }
}

public enum CoefficientBlendMode
{
    Add,
    Multiply,
    Average
}