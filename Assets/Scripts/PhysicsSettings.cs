using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PhysicsSettings", menuName = "Physics Settings")]
public class PhysicsSettings : ScriptableObject
{
    public static PhysicsSettings CurrentSettings { get; private set; }

    [SerializeField]
    private bool isEnabled = false;

    public Vector3 gravity = new Vector3(0, -9.8f, 0);

    private void OnEnable()
    {
        if (!isEnabled) return;

        CurrentSettings = this;
        Debug.Log("Physics settings enabled.");
    }
}
