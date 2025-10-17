using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
   
    public Rigidbody2D rb;
    public Transform ballSpawnPosition;
    public LineRenderer lineRenderer;
    public SpriteRenderer powerUpSpriteRenderer;

    public int maxBounces = 5;
    public float launchForce;
    public float maxLaunchForcse = 10f;
    public float powerRefillRate;
    public float rotationSpeed = 5f;
    private bool MouseHeld => Input.GetMouseButton(0);
    private bool MouseReleased => Input.GetMouseButtonUp(0);
    private bool MousePressed => Input.GetMouseButtonDown(0);
    private void Start()
    {
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, rb.transform.position);
    }
    private void HandleMouseInputs() 
    {
        if (MousePressed)
        {
            launchForce = 0f;
            Color alpha = powerUpSpriteRenderer.color;
            alpha.a = 0;
            powerUpSpriteRenderer.color = alpha;
            powerUpSpriteRenderer.transform.localScale = Vector3.zero;
        }
        if (MouseHeld)
        {
            if (launchForce < maxLaunchForcse)
            {
                launchForce += powerRefillRate * Time.deltaTime;
                var relativePower = launchForce / maxLaunchForcse;
                Color alpha = powerUpSpriteRenderer.color;
                alpha.a = relativePower;
                powerUpSpriteRenderer.color = alpha;
                powerUpSpriteRenderer.transform.localScale = Vector3.one * relativePower;
            }
            RotateBallTowardsMouse();
        }
        if (MouseReleased)
        {
            LaunchBall();
        }
    }
    private void HandleKeyboardInputs()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetBall();
        }
    }
    private void Update()
    {
        HandleMouseInputs();
        HandleKeyboardInputs();
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