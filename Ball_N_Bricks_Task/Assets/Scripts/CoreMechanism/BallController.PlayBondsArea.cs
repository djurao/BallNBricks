using UnityEngine;

public partial class BallController
{
    // Called from Update() to handle movement and bounce against the play area bounds.
    // Behavior and thresholds match your existing implementation.
    private void HandleBoundsCollision()
    {
        if (!isLaunched || ballTransform == null || launchVelocity.sqrMagnitude <= 0f || playAreaCollider == null)
            return;
        var pos = (Vector2)ballTransform.position;
        var nextPos = pos + launchVelocity * Time.deltaTime;

        // compute inset bounds so the ball stays fully inside
        var b = playAreaCollider.bounds;
        var inset = ballRadius;
        var minX = b.min.x + inset;
        var maxX = b.max.x - inset;
        var minY = b.min.y + inset;
        var maxY = b.max.y - inset;

        var collided = false;
        var collisionNormal = Vector2.zero;

        // check vertical walls (left/right)
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

        // check horizontal walls (bottom/top)
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

            // preserve incoming speed, reflect direction, then apply restitution to speed
            float speed = launchVelocity.magnitude;
            Vector2 reflectedDir = Vector2.Reflect(launchVelocity.normalized, collisionNormal);
            launchVelocity = reflectedDir * speed * restitution;

            // rotate ball to face movement
            float angle = Mathf.Atan2(launchVelocity.y, launchVelocity.x) * Mathf.Rad2Deg - 90f;
            if (ballTransform != null)
                ballTransform.rotation = Quaternion.Euler(0f, 0f, angle);

            // small inward nudge to avoid sticking to edge due to precision
            nextPos += collisionNormal * 0.001f;
        }

        if (ballTransform != null)
            ballTransform.position = new Vector3(nextPos.x, nextPos.y, ballTransform.position.z);

        // stop when speed very small
        if (launchVelocity.sqrMagnitude < 0.01f)
        {
            launchVelocity = Vector2.zero;
            isLaunched = false;
        }
    }
}