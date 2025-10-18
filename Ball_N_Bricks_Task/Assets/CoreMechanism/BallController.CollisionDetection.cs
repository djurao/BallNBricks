using System.Collections.Generic;
using UnityEngine;

public partial class BallController : MonoBehaviour
{
    public LayerMask brickLayerMask;
    public float collisionCheckExtra = 0.01f;

    private void CheckBallBrickCollisions()
    {
        if (ballTransform == null) return;

        // compute ball center and radius in world space
        Vector2 center = (Vector2)ballTransform.position;
        float radius = ballRadius;
        if (ballCollider != null)
        {
            float scale = Mathf.Max(Mathf.Abs(ballTransform.lossyScale.x), Mathf.Abs(ballTransform.lossyScale.y));
            radius = ballCollider.radius * scale;
            center = (Vector2)ballTransform.TransformPoint(ballCollider.offset);
        }

        float checkRadius = radius + collisionCheckExtra;

        // find brick colliders overlapping the ball
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, checkRadius, brickLayerMask);
        if (hits == null || hits.Length == 0) return;

        foreach (var col in hits)
        {
            if (col == null) continue;

            var brick = col.GetComponentInParent<Brick>() ?? col.GetComponentInChildren<Brick>();
            if (brick == null) continue;

            // Notify brick
            brick.OnHit();

            // Compute collision normal: from closest point on collider to ball center
            Vector2 ballCenter = center;
            Vector2 closest = col.ClosestPoint(ballCenter);
            Vector2 normal = ballCenter - closest;

            if (normal.sqrMagnitude < 1e-6f)
            {
                // fallback: approximate normal from collider bounds center toward ball
                normal = ballCenter - (Vector2)col.bounds.center;
                if (normal.sqrMagnitude < 1e-6f)
                    normal = Vector2.up;
            }

            normal.Normalize();

            // Reflect velocity and apply restitution
            float speed = launchVelocity.magnitude;
            if (speed > 1e-6f)
            {
                Vector2 reflectedDir = Vector2.Reflect(launchVelocity.normalized, normal);
                launchVelocity = reflectedDir * speed * restitution;

                // rotate ball to face movement
                float angle = Mathf.Atan2(launchVelocity.y, launchVelocity.x) * Mathf.Rad2Deg - 90f;
                ballTransform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            // Nudge ball outward along normal to avoid immediate re-hit
            Vector2 newPos = ballCenter + normal * (radius + 0.001f);
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