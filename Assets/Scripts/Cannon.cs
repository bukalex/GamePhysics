using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class Cannon : MonoBehaviour
{
    public static float[] ranges;
    public static float[] times;

    [SerializeField]
    private Parcel projectilePrefab;
    [SerializeField]
    private float launchAngle = 30;
    [SerializeField]
    private float launchSpeed = 10;
    [SerializeField]
    private string path;
    [SerializeField]
    private string resultPath;

    [SerializeField]
    TMP_Text angleText;
    [SerializeField]
    TMP_Text rangeText;
    [SerializeField]
    TMP_Text timeText;

    private float[] angles;
    private Vector3 initialVelocity;
    private int angleIndex = 0;

    private void Awake()
    {
        path = "Assets/Resources/" + path;
        resultPath = "Assets/Resources/" + resultPath;

        string[] anglesString = File.ReadAllLines(path);
        angles = new float[anglesString.Length];
        ranges = new float[anglesString.Length];
        times = new float[anglesString.Length];

        for (int i = 0; i < anglesString.Length; i++)
        {
            angles[i] = float.Parse(anglesString[i]);
        }

        launchAngle = angles[angleIndex];
        initialVelocity = CalculateInitialVelocity();
        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LaunchParcel();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            foreach (float angle in angles)
            {
                angleIndex = (angles.Length + angleIndex + 1) % angles.Length;
                launchAngle = angles[angleIndex];

                LaunchParcel();
            }
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            angleIndex = (angles.Length + angleIndex + (int)Input.mouseScrollDelta.y) % angles.Length;
            launchAngle = angles[angleIndex];

            LaunchParcel();
            UpdateUI();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + initialVelocity.normalized);
    }

    private void OnValidate()
    {
        initialVelocity = CalculateInitialVelocity();
    }

    private Vector3 CalculateInitialVelocity()
    {
        Vector3 velocity = new Vector3(
                Mathf.Cos(launchAngle * Mathf.Deg2Rad),
                Mathf.Sin(launchAngle * Mathf.Deg2Rad),
                0);
        velocity *= launchSpeed;

        return velocity;
    }

    private float CalculateRange(float angle, float speed, float gravity)
    {
        float range = speed * speed;
        range *= Mathf.Sin(angle * 2 * Mathf.Deg2Rad);
        range /= gravity;

        return range;
    }

    private float CalculateFlightTime(Vector3 velocity, float gravity)
    {
        return 2 * velocity.y / gravity;
    }

    private void UpdateUI()
    {
        angleText.text = "Current angle (deg): " + launchAngle;
        rangeText.text = "Expected range (m): " + CalculateRange(launchAngle, launchSpeed, PhysicsSettings.CurrentSettings.gravity.magnitude).ToString("F2");
        timeText.text = "Expected flight time (s): " + CalculateFlightTime(initialVelocity, PhysicsSettings.CurrentSettings.gravity.magnitude).ToString("F2");
    }

    private void LaunchParcel()
    {
        Parcel projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        initialVelocity = CalculateInitialVelocity();
        projectile.SetVelocity(initialVelocity, angleIndex);
    }

    private void OnApplicationQuit()
    {
        int firstColWidth = 14;
        int width = 10;

        string testLine = AddColumn("", "Test", firstColWidth);
        string angleLine = AddColumn("", "Angle (deg)", firstColWidth);
        string rangeLine = AddColumn("", "Range (m)", firstColWidth);
        string timeLine = AddColumn("", "Time (s)", firstColWidth);

        for (int i = 0; i < angles.Length; i++) testLine = AddColumn(testLine, (i + 1).ToString(), width);
        foreach(float angle in angles) angleLine = AddColumn(angleLine, angle.ToString("F3"), width);
        foreach (float range in ranges) rangeLine = AddColumn(rangeLine, range.ToString("F2"), width);
        foreach (float time in times) timeLine = AddColumn(timeLine, time.ToString("F2"), width);

        if (File.Exists(resultPath)) File.Delete(resultPath);

        StreamWriter writer = File.CreateText(resultPath);
        writer.WriteLine(testLine);
        writer.WriteLine(angleLine);
        writer.WriteLine(rangeLine);
        writer.WriteLine(timeLine);
        writer.Close();
    }

    private string AddColumn(string row, string value, int width)
    {
        string cell = "|";

        width = (width - value.Length) >= 0 ? (width - value.Length) : 0;
        for (int i = 0; i < width / 2; i++) cell += " ";
        cell += value;
        for (int i = 0; i < width - width / 2; i++) cell += " ";
        cell += "|";

        return row + cell;
    }
}
