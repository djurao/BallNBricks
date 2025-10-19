using System;
using System.Collections.Generic;
using UnityEngine;

public partial class BallController : MonoBehaviour
{
    public LayerMask brickLayerMask;
    public float collisionCheckExtra = 0.01f;
    public event Action OnCollidedWithBrick;

    private void CheckBallBrickCollisions()
    {
        if (ballTransform == null) return;

        // compute ball center and radius in world space
        var center = (Vector2)ballTransform.position;
        var radius = ballRadius;
        if (ballCollider != null)
        {
            var scale = Mathf.Max(Mathf.Abs(ballTransform.lossyScale.x), Mathf.Abs(ballTransform.lossyScale.y));
            radius = ballCollider.radius * scale;
            center = (Vector2)ballTransform.TransformPoint(ballCollider.offset);
        }

        var checkRadius = radius + collisionCheckExtra;

        // find brick colliders overlapping the ball
        var hits = Physics2D.OverlapCircleAll(center, checkRadius, brickLayerMask);
        if (hits == null || hits.Length == 0) return;

        foreach (var col in hits)
        {
            if (col == null) continue;

            var brick = col.GetComponentInParent<Brick>() ?? col.GetComponentInChildren<Brick>();
            if (brick == null) continue;

            // Notify brick
            brick.OnHit();
            OnCollidedWithBrick?.Invoke();
            // Compute collision normal: from closest point on collider to ball center
            var ballCenter = center;
            var closest = col.ClosestPoint(ballCenter);
            var normal = ballCenter - closest;

            if (normal.sqrMagnitude < 1e-6f)
            {
                // fallback: approximate normal from collider bounds center toward ball
                normal = ballCenter - (Vector2)col.bounds.center;
                if (normal.sqrMagnitude < 1e-6f)
                    normal = Vector2.up;
            }

            normal.Normalize();

            // Reflect velocity and apply restitution
            var speed = launchVelocity.magnitude;
            if (speed > 1e-6f)
            {
                var reflectedDir = Vector2.Reflect(launchVelocity.normalized, normal);
                launchVelocity = reflectedDir * speed * restitution;

                // rotate ball to face movement
                var angle = Mathf.Atan2(launchVelocity.y, launchVelocity.x) * Mathf.Rad2Deg - 90f;
                ballTransform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            // Nudge ball outward along normal to avoid immediate re-hit
            var newPos = ballCenter + normal * (radius + 0.001f);
            ballTransform.position = new Vector3(newPos.x, newPos.y, ballTransform.position.z);

            // If velocity very small, stop the ball
            if (launchVelocity.sqrMagnitude < 0.01f)
            {
                launchVelocity = Vector2.zero;
                isLaunched = false;
            }
        }
    }

}