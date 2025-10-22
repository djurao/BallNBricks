using UnityEngine;

public partial class BallController
{
    private void HandleBoundsCollision()
    {
        if (!isLaunched || ballTransform == null || launchVelocity.sqrMagnitude <= 0f || playAreaCollider == null)
            return;
        var pos = (Vector2)ballTransform.position;
        var nextPos = pos + launchVelocity * Time.deltaTime;

        var b = playAreaCollider.bounds;
        var inset = ballRadius;
        var minX = b.min.x + inset;
        var maxX = b.max.x - inset;
        var minY = b.min.y + inset;
        var maxY = b.max.y - inset;

        var collided = false;
        var collisionNormal = Vector2.zero;

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

            var speed = launchVelocity.magnitude;
            var reflectedDir = Vector2.Reflect(launchVelocity.normalized, collisionNormal);
            launchVelocity = reflectedDir * speed * restitution;

            var angle = Mathf.Atan2(launchVelocity.y, launchVelocity.x) * Mathf.Rad2Deg - 90f;
            if (ballTransform != null)
                ballTransform.rotation = Quaternion.Euler(0f, 0f, angle);

            nextPos += collisionNormal * 0.001f;
        }

        if (ballTransform != null)
            ballTransform.position = new Vector3(nextPos.x, nextPos.y, ballTransform.position.z);

        if (launchVelocity.sqrMagnitude < 0.01f)
        {
            launchVelocity = Vector2.zero;
            isLaunched = false;
        }
    }
}