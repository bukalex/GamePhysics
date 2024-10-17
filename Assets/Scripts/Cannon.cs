using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [SerializeField]
    private PhysicsBody projectilePrefab;
    [SerializeField]
    private float launchAngle = 30;
    [SerializeField]
    private float launchSpeed = 10;
    [SerializeField]
    private string path;

    private float[] angles;

    private void Awake()
    {
        path = "Assets/Resources/" + path;

        if (File.Exists(path))
        {
            string[] anglesString = File.ReadAllLines(path);
            angles = new float[anglesString.Length];

            for (int i = 0; i < anglesString.Length; i++)
            {
                angles[i] = float.Parse(anglesString[i]);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PhysicsBody projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            Vector3 initialVelocity = new Vector3(
                Mathf.Cos(launchAngle * Mathf.Deg2Rad),
                Mathf.Sin(launchAngle * Mathf.Deg2Rad), 
                0);
            initialVelocity *= launchSpeed;
            projectile.Velocity = initialVelocity;
        }
    }
}
