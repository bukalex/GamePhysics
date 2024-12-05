using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sling : MonoBehaviour
{
    public enum SlingState
    {
        Ready,
        Aiming,
        Waiting
    }

    [SerializeField]
    private Bird[] birdPrefabs;

    [SerializeField]
    private Transform origin;

    [SerializeField]
    private float impulsePerUnit = 10;

    [SerializeField]
    private float maxPullDistance = 3;

    [SerializeField]
    private float mouseSensitivity = 3;

    [SerializeField]
    private LineRenderer lineRenderer;

    [SerializeField]
    private LineRenderer previousLineRenderer;

    private Bird currentBird;
    private int currentBirdIndex = 0;
    private SlingState state = SlingState.Waiting;
    private Vector3 initialMousePosition;
    private Vector3 mouseDelta;
    private Vector3[] positions;

    void Update()
    {
        switch (state)
        {
            case SlingState.Ready:
                if (Input.GetMouseButtonDown(0))
                {
                    initialMousePosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                    state = SlingState.Aiming;

                    lineRenderer.enabled = true;
                }
                else if (Input.GetKeyDown(KeyCode.Tab))
                {
                    ChangeBird();
                }
                break;

            case SlingState.Aiming:
                if (Input.GetMouseButton(0))
                {
                    mouseDelta = Camera.main.ScreenToViewportPoint(Input.mousePosition) - initialMousePosition;
                    mouseDelta = Vector3.ClampMagnitude(mouseDelta * mouseSensitivity, maxPullDistance);
                    currentBird.transform.position = origin.position + mouseDelta;

                    positions = PhysicsSystem.PredictPath(currentBird.transform.position, -mouseDelta * impulsePerUnit / currentBird.GetMass(), currentBird.GetComponent<PhysicsShape>(), 35);
                    lineRenderer.positionCount = positions.Length;
                    lineRenderer.SetPositions(positions);

                    if (Input.GetMouseButtonDown(1))
                    {
                        state = SlingState.Ready;
                    }
                    else if (Input.GetKeyDown(KeyCode.Tab))
                    {
                        ChangeBird();
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    currentBird.Launch(-mouseDelta * impulsePerUnit);
                    state = SlingState.Waiting;

                    lineRenderer.enabled = false;
                    previousLineRenderer.enabled = true;
                    previousLineRenderer.positionCount = positions.Length;
                    previousLineRenderer.SetPositions(positions);
                }
                break;

            case SlingState.Waiting:
                if (!currentBird)
                {
                    currentBird = Instantiate(birdPrefabs[currentBirdIndex], origin.position, Quaternion.identity);
                    state = SlingState.Ready;
                }
                break;
        }
    }

    private void ChangeBird()
    {
        currentBirdIndex++;
        currentBirdIndex %= birdPrefabs.Length;

        if (currentBird) Destroy(currentBird.gameObject);
        currentBird = Instantiate(birdPrefabs[currentBirdIndex], origin.position, Quaternion.identity);
    }
}
