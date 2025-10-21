using System.Collections;
using System.Collections.Generic;
using CoreMechanism;
using Misc;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(CircleCollider2D))]
public class BallMovement : MonoBehaviour
{
    private BallController ballController;
    // Runtime state (set by caller or by calling LaunchBall)
    [HideInInspector] public Vector2 velocity = Vector2.zero;
    [HideInInspector] public bool isLaunched = false;
    public SpriteRenderer spriteRenderer;
    public Sprite normalBall;
    public Sprite chromeBall;
// References (assign in inspector or set by BallController at runtime)
    public BoxCollider2D playAreaCollider;
    public LayerMask brickLayerMask;
    public float collisionCheckExtra = 0.01f;
    public float ballRadius = 0.25f; // world units
    [Range(0f, 1f)] public float restitution = 0.8f;

    private CircleCollider2D circleCollider;
    private float bottomBoundaryY;

    public Transform spawnPoint;
    public Transform basketPoint;
    private const float stopSpeedSqr = 0.01f;

// Coroutine handle for moving to basket
    private Coroutine moveToBasketCoroutine;

    void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null) circleCollider = gameObject.AddComponent<CircleCollider2D>();
    }

    public void DI(BallController _ballController)
    {
        ballController = _ballController;
    }

    void Start()
    {
        if (playAreaCollider != null)
        {
            var bounds = playAreaCollider.bounds;
            bottomBoundaryY = bounds.min.y;
        }
    }

// Call this to start a launch from outside (BallController)
// direction should be normalized; speed is world units/sec
    public void LaunchBall(Vector2 direction, float speed)
    {
        spriteRenderer.sprite = !PowerUps.Instance.chromeBallActive ? normalBall : chromeBall;  
        transform.position = spawnPoint.position;
        if (direction.sqrMagnitude < 1e-6f || speed <= 0f) return;
        velocity = direction.normalized * speed;
        isLaunched = true;
    }

    public void ResetBall()
    {
        // Stop any ongoing basket coroutine
        if (moveToBasketCoroutine != null)
        {
            StopCoroutine(moveToBasketCoroutine);
            moveToBasketCoroutine = null;
        }

        // Reset position to spawn point
        if (spawnPoint != null)
            transform.position = spawnPoint.position;

        // Reset velocities and states
        velocity = Vector2.zero;
        isLaunched = false;
        // Reset rotation if needed
        transform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        if (isLaunched)
            UpdateMovement();
    }

    public void MoveToBasket()
    {
        // Stop any existing coroutine and start a new one
        if (moveToBasketCoroutine != null) StopCoroutine(moveToBasketCoroutine);
        // Zero velocity immediately to avoid further physics movement
        velocity = Vector2.zero;
        moveToBasketCoroutine = StartCoroutine(MoveToBasketAndStop(basketPoint.position, 0.6f));
    }

    public void UpdateMovement()
    {
        if (!isLaunched || velocity.sqrMagnitude <= 0f || playAreaCollider == null) return;

        Vector2 pos = (Vector2)transform.position;
        Vector2 nextPos = pos + velocity * Time.deltaTime;

        var b = playAreaCollider.bounds;
        float inset = (circleCollider != null)
            ? Mathf.Max(0.0001f,
                circleCollider.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y)))
            : ballRadius;

        float minX = b.min.x + inset;
        float maxX = b.max.x - inset;
        float minY = b.min.y + inset;
        float maxY = b.max.y - inset;

        // Check if ball hits the bottom edge -> interpolate to basketPoint and stop
        if (nextPos.y <= bottomBoundaryY + inset)
        {
            if (basketPoint != null)
            {
                MoveToBasket();
            }
            else
            {
                ResetBall();
            }

            return; // Exit early after scheduling the move
        }

        bool collided = false;
        Vector2 collisionNormal = Vector2.zero;

        // Handle bounds collisions
        if (nextPos.x < minX)
        {
            nextPos.x = minX;
            collisionNormal += Vector2.right;
            collided = true;
        }
        else if (nextPos.x > maxX)
        {
            nextPos.x = maxX;
            collisionNormal += Vector2.left;
            collided = true;
        }

        if (nextPos.y < minY)
        {
            nextPos.y = minY;
            collisionNormal += Vector2.up;
            collided = true;
        }
        else if (nextPos.y > maxY)
        {
            nextPos.y = maxY;
            collisionNormal += Vector2.down;
            collided = true;
        }

        if (collided)
        {
            collisionNormal = collisionNormal.normalized;
            float speed = velocity.magnitude;
            Vector2 reflectedDir = Vector2.Reflect(velocity.normalized, collisionNormal);
            velocity = reflectedDir * speed * restitution;

            // rotate to face movement
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            // small inward nudge
            nextPos += collisionNormal * 0.001f;
        }

        // Apply calculated position
        transform.position = new Vector3(nextPos.x, nextPos.y, transform.position.z);

        // Handle brick collisions
        HandleBrickCollisions();

        // Stop if velocity is very small
        if (velocity.sqrMagnitude < stopSpeedSqr)
        {
            velocity = Vector2.zero;
            isLaunched = false;
        }
    }

    IEnumerator MoveToBasketAndStop(Vector3 target, float duration)
    {
        float t = 0f;
        Vector3 start = transform.position;
        Quaternion startRot = transform.rotation;
        // Optional: aim rotation to face downward at the end (adjust as desired)
        Vector3 directionToTarget = (target - start).normalized;
        float targetAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle);

        // Smoothly move and optionally rotate
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.SmoothStep(0f, 1f, t / duration);
            transform.position = Vector3.Lerp(start, target, alpha);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, alpha);
            yield return null;
        }

        transform.position = target;
        transform.rotation = targetRot;
        velocity = Vector2.zero;
        moveToBasketCoroutine = null;
        if(!retracted && isLaunched)ballController.onBallRetracted?.Invoke();
        retracted = true;
        isLaunched = false;
    }

    public bool retracted;
    void HandleBrickCollisions()
    {
        if (brickLayerMask == 0) return;

        Vector2 center = (circleCollider != null)
            ? (Vector2)transform.TransformPoint(circleCollider.offset)
            : (Vector2)transform.position;

        float radius = (circleCollider != null)
            ? circleCollider.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y))
            : ballRadius;

        float checkRadius = radius + collisionCheckExtra;

        var hits = Physics2D.OverlapCircleAll(center, checkRadius, brickLayerMask);
        if (hits == null || hits.Length == 0) return;

        foreach (var col in hits)
        {
            if (col == null) continue;

            var collectable = col.GetComponentInParent<Collectable>() ?? col.GetComponentInChildren<Collectable>();
            if (collectable != null)
                collectable.OnCollect();
                
            var brick = col.GetComponentInParent<Brick>() ?? col.GetComponentInChildren<Brick>();
            if (brick == null) continue;
            
           

            // Notify the brick it was hit
            brick.OnHit(PowerUps.Instance.chromeBallActive);
            if (PowerUps.Instance.chromeBallActive)
                continue; // so that we ignore bouncing

            // Compute normal using collider's ClosestPoint
            Vector2 closest = col.ClosestPoint(center);
            Vector2 normal = center - closest;
            if (normal.sqrMagnitude < 1e-6f) normal = center - (Vector2)col.bounds.center;

            normal = normal.sqrMagnitude > 1e-6f ? normal.normalized : Vector2.up;

            // Reflect velocity and apply bounce
            float speed = velocity.magnitude;
            if (speed > 1e-6f)
            {
                Vector2 reflectedDir = Vector2.Reflect(velocity.normalized, normal);
                velocity = reflectedDir * speed * restitution;

                // Rotate to face movement
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            // Move ball out of collision
            Vector2 newPos = center + normal * (radius + 0.001f);
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }
    }
}