using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public partial class BallController : MonoBehaviour
{
    private static readonly int Swing = Animator.StringToHash("Swing");
    private static readonly int StopSwinging = Animator.StringToHash("StopSwinging");

    [Header("References (assign separate GameObject each)")]
    public Transform ballSpawnPosition;

    public Transform ballTransform;
    public CircleCollider2D  ballCollider;
    public SpriteRenderer powerUpSpriteRenderer;
    public Camera mainCamera;
    public List<BallMovement> ballMovements = new List<BallMovement>();
    public Animator swingAnimator;
    public GameObject retractBallsButton;
    public int ballsInAction;
    public int ballsLaunched;
    
    [Header("Launch / Input")] public float launchForce = 0f;
    public float maxLaunchForce = 10f;
    public float powerRefillRate = 5f;
    public float rotationSpeed = 8f;
    private bool batInteractable;
    private Vector2 initialLaunchVelocity;
    private bool isVelocityCaptured = false;
    private bool CanLaunchBalls => ballsInAction == 0 && ballsLaunched == 0;

    private Vector2 launchVelocity = Vector2.zero;
    public bool isLaunched;
    private Coroutine launchRoutine;
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
    public Action onBallRetracted;
    private void OnEnable() => onBallRetracted += OnBallRetracted;
    private void OnDisable() => onBallRetracted -= OnBallRetracted;

    private void Awake() => InitBalls();

    private void InitBalls()
    {
        foreach (var ball in ballMovements)
        {
            ball.DI(this);
        }
    }
    public void PrepareBatAndBallLogic()
    {
        Invoke(nameof(UnlockBatInteraction), 0.5f);
    }
    // TODO Create method to Lock it after level is complete
    private void UnlockBatInteraction() => batInteractable = true;
    private void OnBallRetracted()
    {
        ballsInAction--;
        if (ballsInAction == 0)
        {
            retractBallsButton.SetActive(false);
            ballsLaunched = 0;
        }
    }

    public void LaunchBalls()
    {
        if (ballsInAction > 0) return;
        ballsLaunched = 0;
        retractBallsButton.SetActive(true);
        swingAnimator.SetTrigger(Swing);
        StopBallsLaunch();
        PlayAudioBatHit();
        launchRoutine = StartCoroutine(LaunchBallsSequential());
    }

    private void StopBallsLaunch()
    {
        if (launchRoutine != null)
        {
            StopCoroutine(launchRoutine);
        }
    }

    private IEnumerator LaunchBallsSequential()
    {
        isVelocityCaptured = false; // Reset at the start
        foreach (var ballMovement in ballMovements)
        {
            if (ballMovement != null)
            {
                if (mainCamera != null && ballTransform != null)
                {
                    float zDistance = Mathf.Abs(mainCamera.transform.position.z - ballTransform.position.z);
                    Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDistance));
                    mouseWorld.z = ballTransform.position.z;
                    Vector3 dir3 = (mouseWorld - ballTransform.position);
                    Vector2 direction = new Vector2(dir3.x, dir3.y).normalized;

                    if (direction.sqrMagnitude >= 1e-6f)
                    {
                        float speed = launchForce; // or any value you want
                        if (!isVelocityCaptured)
                        {
                            // Save the velocity of the first launched ball
                            initialLaunchVelocity = direction * speed;
                            isVelocityCaptured = true;
                        }
                        // Launch all balls with this stored velocity
                        ballMovement.LaunchBall(initialLaunchVelocity.normalized, initialLaunchVelocity.magnitude);
                        ballMovement.retracted = false;
                        ballsInAction++;
                        ballsLaunched++;
                        if (ballsLaunched ==  ballMovements.Count)
                        {
                            swingAnimator.SetTrigger(StopSwinging);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(0.1f); // wait before launching next
        }
    }
    private void PlayAudioBatHit() => AudioManager.Instance.PlayBallHit();
    private void Update()
    {
        if (!CanLaunchBalls || !batInteractable) return;
        UpdateTrajectoryIfNeeded();
        HandleMouseInput();

        if (MouseHeld)
            RotateBallTowardsMouse();

        HandleBoundsCollision();
        CheckBallBrickCollisions();
    }

    private void HandleMouseInput()
    {
        if (ballsInAction > 0) return;
        if (MousePressed) OnBeginCharge();
        if (MouseHeld) ContinueCharge();
        if (MouseReleased) LaunchBalls();
    }
    public void RetractAllBallsToBase()
    {
        StopCoroutine(launchRoutine);
        ballsLaunched = 0;
        swingAnimator.SetTrigger(StopSwinging);
        foreach (var ball in ballMovements)
        {
            if (ball != null)
            {
                ball.MoveToBasket();
            }
        }
    }

    private void ResetBall()
    {
        foreach (var ball in ballMovements)
        {
            ball.ResetBall();
        }
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
        //powerUpSpriteRenderer.transform.localScale = Vector3.one * Mathf.Clamp01(normalized);
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