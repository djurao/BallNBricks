using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BallController : MonoBehaviour
{
    [Header("References (assign separate GameObject each)")]
    public Transform ballSpawnPosition;

    public Transform ballTransform;
    public CircleCollider2D  ballCollider;
    public SpriteRenderer powerUpSpriteRenderer;
    public Camera mainCamera;
    [Header("Launch / Input")] public float launchForce = 0f;
    public float maxLaunchForce = 10f;
    public float powerRefillRate = 5f;
    public float rotationSpeed = 8f;
    public float currentSpeed;
// transform-based velocity (2D)
    private Vector2 launchVelocity = Vector2.zero;
    public bool isLaunched;

    [Header("Play area & collision")] public BoxCollider2D playAreaCollider; // single box play area
    public float ballRadius = 0.25f; // world units
    [Header("Play area & collision")]
    [Tooltip("Bounciness: 1 = preserve speed on bounce, 0 = no bounce (stop). Values between 0 and 1 scale outgoing speed.")]
    [Range(0f, 1f)]
    public float restitution = 0.8f;

// Input helpers
    private bool MouseHeld => Input.GetMouseButton(0);
    private bool MouseReleased => Input.GetMouseButtonUp(0);
    private bool MousePressed => Input.GetMouseButtonDown(0);
    private void UpdateSpeedValue()
    {
        currentSpeed = launchVelocity.magnitude;
    }
    private void Update()
    {
        UpdateTrajectoryIfNeeded();
        UpdateSpeedValue();
        HandleKeyboardInput();
        HandleMouseInput();

        if (MouseHeld)
            RotateBallTowardsMouse();

        HandleBoundsCollision();
        CheckBallBrickCollisions();
    }

    private void HandleMouseInput()
    {
        if (MousePressed) OnBeginCharge();
        if (MouseHeld) ContinueCharge();
        if (MouseReleased) LaunchBall();
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.R)) ResetBall();
    }

    private void OnBeginCharge()
    {
        launchForce = 0f;
        SetPowerUI(0f);
    }

    private void ContinueCharge()
    {
        launchForce = Mathf.Min(launchForce + powerRefillRate * Time.deltaTime, maxLaunchForce);
        var normalized = (maxLaunchForce > 0f) ? (launchForce / maxLaunchForce) : 0f;
        SetPowerUI(normalized);
    }

    private void SetPowerUI(float normalized)
    {
        if (powerUpSpriteRenderer == null) return;
        var color = powerUpSpriteRenderer.color;
        color.a = Mathf.Clamp01(normalized);
        powerUpSpriteRenderer.color = color;
        powerUpSpriteRenderer.transform.localScale = Vector3.one * Mathf.Clamp01(normalized);
    }

    private void LaunchBall()
    {
        if (ballTransform == null || mainCamera == null) return;

        float zDistance = Mathf.Abs(mainCamera.transform.position.z - ballTransform.position.z);
        Vector3 mouseWorld =
            mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDistance));
        mouseWorld.z = ballTransform.position.z;
        Vector3 dir3 = (mouseWorld - ballTransform.position);
        Vector2 direction = new Vector2(dir3.x, dir3.y).normalized;
        if (direction.sqrMagnitude < 1e-6f) return;

        launchVelocity = direction * launchForce;
        isLaunched = true;

        SetPowerUI(0f);
        launchForce = 0f;
    }

    private void ResetBall()
    {
        if (ballTransform == null) return;

        if (ballSpawnPosition != null)
            ballTransform.position = ballSpawnPosition.position;

        ballTransform.rotation = Quaternion.Euler(0f, 0f, 0);
        launchVelocity = Vector2.zero;
        isLaunched = false;
        SetPowerUI(0f);
    }
    private void OnValidate()
    {
        restitution = Mathf.Clamp01(restitution);
        ballRadius = Mathf.Max(0f, ballRadius);
        maxLaunchForce = Mathf.Max(0f, maxLaunchForce);
        powerRefillRate = Mathf.Max(0f, powerRefillRate);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
    }
    private void RotateBallTowardsMouse()
    {
        if (mainCamera == null || ballTransform == null) return;

        float zDistance = Mathf.Abs(mainCamera.transform.position.z - ballTransform.position.z);
        Vector3 mouseWorld =
            mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDistance));
        mouseWorld.z = ballTransform.position.z;

        var dir = mouseWorld - ballTransform.position;
        if (dir.sqrMagnitude < 1e-6f) return;

        var targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        var currentZ = ballTransform.rotation.eulerAngles.z;
        var newZ = Mathf.LerpAngle(currentZ, targetAngle, rotationSpeed * Time.deltaTime);
        ballTransform.rotation = Quaternion.Euler(0f, 0f, newZ);
    }
}