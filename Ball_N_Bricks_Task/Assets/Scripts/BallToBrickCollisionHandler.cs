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
            if (brick != null)
            {
                brick.OnHit();
            }
        }
    }
}