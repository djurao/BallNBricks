using System.Collections;
using Misc;
using UnityEngine;

namespace CoreMechanism
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class BallMovement : MonoBehaviour
    {
        private BallController ballController;
        [HideInInspector] public Vector2 velocity = Vector2.zero;
        public SpriteRenderer spriteRenderer;
        public Sprite normalBall;
        public Sprite chromeBall;
        public BoxCollider2D playAreaCollider;
        public LayerMask brickLayerMask;
        public float collisionCheckExtra = 0.01f;
        public float ballRadius = 0.25f;
        private float bottomBoundaryY;
        private const float stopSpeedSqr = 0.01f;
        public bool retracted;
        [HideInInspector] public bool isLaunched = false;
        [Range(0f, 1f)] public float restitution = 0.8f;
        private CircleCollider2D circleCollider;
        public Transform spawnPoint;
        public Transform basketPoint;
        private Coroutine moveToBasketCoroutine;

        void Awake()
        {
            circleCollider = GetComponent<CircleCollider2D>();
            if (circleCollider == null) circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        public void DI(BallController _ballController, Transform spawnP, BoxCollider2D boxCollider2D,
            Transform basketPosition)
        {
            spawnPoint = spawnP;
            ballController = _ballController;
            playAreaCollider = boxCollider2D;
            basketPoint = basketPosition;
        }
        void Start()
        {
            if (playAreaCollider == null) return;
            var bounds = playAreaCollider.bounds;
            bottomBoundaryY = bounds.min.y;
        }
        public void LaunchBall(Vector2 direction, float speed)
        {
            spriteRenderer.sprite = !PowerUps.Instance.chromeBallActive ? normalBall : chromeBall;  
            transform.position = spawnPoint.position;
            if (direction.sqrMagnitude < 1e-6f || speed <= 0f) return;
            velocity = direction.normalized * speed;
            isLaunched = true;
        }
        private void ResetBall()
        {
            if (moveToBasketCoroutine != null)
            {
                StopCoroutine(moveToBasketCoroutine);
                moveToBasketCoroutine = null;
            }

            if (spawnPoint != null)
                transform.position = spawnPoint.position;

            velocity = Vector2.zero;
            isLaunched = false;
            transform.rotation = Quaternion.identity;
        }
        private void Update()
        {
            if (isLaunched)
                UpdateMovement();
        }
        public void MoveToBasket()
        {
            if (moveToBasketCoroutine != null) StopCoroutine(moveToBasketCoroutine);
            velocity = Vector2.zero;
            moveToBasketCoroutine = StartCoroutine(MoveToBasketAndStop(basketPoint.position, 0.6f));
        }
        private void UpdateMovement()
        {
            if (!isLaunched || velocity.sqrMagnitude <= 0f || playAreaCollider == null) return;

            var pos = (Vector2)transform.position;
            var nextPos = pos + velocity * Time.deltaTime;

            var b = playAreaCollider.bounds;
            var inset = (circleCollider != null)
                ? Mathf.Max(0.0001f,
                    circleCollider.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y)))
                : ballRadius;

            var minX = b.min.x + inset;
            var maxX = b.max.x - inset;
            var minY = b.min.y + inset;
            var maxY = b.max.y - inset;

            // Bottom edge hit check
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
                return; 
            }

            var collided = false;
            var collisionNormal = Vector2.zero;

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
                var speed = velocity.magnitude;
                var reflectedDir = Vector2.Reflect(velocity.normalized, collisionNormal);
                velocity = reflectedDir * speed * restitution;

                // rotate to face movement
                var angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);

                // small inward nudge, so that we do not trigger multiple collisions
                nextPos += collisionNormal * 0.001f;
            }

            transform.position = new Vector3(nextPos.x, nextPos.y, transform.position.z);

            HandleBrickCollisions();

            if (!(velocity.sqrMagnitude < stopSpeedSqr)) return;
            velocity = Vector2.zero;
            isLaunched = false;
        }

        private IEnumerator MoveToBasketAndStop(Vector3 target, float duration)
        {
            var t = 0f;
            var start = transform.position;
            var startRot = transform.rotation;
            var directionToTarget = (target - start).normalized;
            var targetAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f;
            var targetRot = Quaternion.Euler(0f, 0f, targetAngle);

            // Smooth Moving and rotating
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
        void HandleBrickCollisions()
        {
            if (brickLayerMask == 0) return;

            var center = (circleCollider != null)
                ? (Vector2)transform.TransformPoint(circleCollider.offset)
                : (Vector2)transform.position;

            var radius = (circleCollider != null)
                ? circleCollider.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y))
                : ballRadius;

            var checkRadius = radius + collisionCheckExtra;

            var hits = Physics2D.OverlapCircleAll(center, checkRadius, brickLayerMask);
            if (hits == null || hits.Length == 0) return;

            foreach (var collider in hits)
            {
                if (collider == null) continue;

                var collectable = collider.GetComponentInParent<Collectable>() ?? collider.GetComponentInChildren<Collectable>();
                if (collectable != null)
                    collectable.OnCollect();
                
                var brick = collider.GetComponentInParent<Brick>() ?? collider.GetComponentInChildren<Brick>();
                if (brick == null) continue;
            
                brick.OnHit(PowerUps.Instance.chromeBallActive);
                if (PowerUps.Instance.chromeBallActive)
                    continue; // so that we ignore bouncing

                var closest = collider.ClosestPoint(center);
                var normal = center - closest;
                if (normal.sqrMagnitude < 1e-6f) normal = center - (Vector2)collider.bounds.center;

                normal = normal.sqrMagnitude > 1e-6f ? normal.normalized : Vector2.up;

                var speed = velocity.magnitude;
                if (speed > 1e-6f)
                {
                    var reflectedDir = Vector2.Reflect(velocity.normalized, normal);
                    velocity = reflectedDir * speed * restitution;

                    var angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
                    transform.rotation = Quaternion.Euler(0f, 0f, angle);
                }

                var newPos = center + normal * (radius + 0.001f);
                transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
            }
        }
    }
}