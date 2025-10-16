using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public float launchForce = 10f;
    public Rigidbody2D rb;
    public Transform ballSpawnPosition;
    public float rotationSpeed = 5f;
    public float initialSpeed = 10f;
    public int maxBounces = 5;
    public float maxRayDistance = 100f;
    public LineRenderer lineRenderer;
    private float lastUpdateTime = 0f;
    private const float updateInterval = 0.1f;
    private Vector2 lastPredictionDirection;
    private const float directionThreshold = 0.01f;
    private Vector3[] pathPoints;
    private int pathPointCount;
    private const int maxPathPoints = 100;
    private const float minUpdateInterval = 0.1f;
    private void Start()
    {
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, rb.transform.position);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LaunchBall();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetBall();
        }

        RotateBallTowardsMouse();

  
    }
    private void DrawDebugLines(List<Vector2> points)
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            Debug.DrawLine(points[i], points[i + 1], Color.yellow, Time.deltaTime);
        }
    }
    public void LaunchBall()
    {
        Vector2 launchDirection = rb.transform.up;
        rb.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);
        lineRenderer.positionCount = 0;
    }

    private void ResetBall()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.position = ballSpawnPosition.position;
        rb.rotation = -45f;
    }

    void RotateBallTowardsMouse()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Camera.main.transform.position.z)));
        Vector2 direction = (new Vector2(mouseWorldPos.x, mouseWorldPos.y) - rb.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = Mathf.LerpAngle(rb.rotation, angle, rotationSpeed * Time.deltaTime);
    }

}