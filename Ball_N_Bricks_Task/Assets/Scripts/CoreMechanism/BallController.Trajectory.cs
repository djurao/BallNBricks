using System.Collections.Generic;
using UnityEngine;

public partial class BallController
{
    [Header("Trajectory (Prediction)")] public LineRenderer trajectoryLine;
    public int trajectoryMaxBounces = 10;
    public int trajectoryMaxSegments = 200;
    public float trajectorySkinDistance = 0.02f;
    public bool trajectoryDebugLogs = false;

    [Header("Prediction Settings")]
    [Tooltip("Length used for each predicted step / visual forward segment (world units).")]
    public float predictionStepLen = 2f;

// small constants
    private const float DefaultIntersectionEpsilon = 1e-6f;
    private const float DefaultVelocityEpsilon = 1e-8f;

    public void UpdateTrajectoryIfNeeded()
    {
        if (!CanLaunchBalls) return;
        if (MouseHeld)
            DrawTrajectory();
        else
            trajectoryLine.positionCount = 0;
    }

    Vector2 GetBallWorldCenter()
    {
        if (ballCollider != null) return (Vector2)ballTransform.TransformPoint(ballCollider.offset);
        return (Vector2)ballTransform.position;
    }

    float GetBallWorldRadius()
    {
        if (ballCollider != null)
        {
            var scale = Mathf.Max(Mathf.Abs(ballTransform.lossyScale.x), Mathf.Abs(ballTransform.lossyScale.y));
            return Mathf.Max(0.0001f, ballCollider.radius * scale);
        }

        return 0.1f;
    }

    Vector2 EstimateDirectionFromCharge()
    {
        var d = ballTransform.up;
        return new Vector2(d.x, d.y).normalized;
    }

    bool ComputeNearestBoxIntersection(Vector2 pos, Vector2 dir, float minX, float maxX, float minY, float maxY,
        out Vector2 hitPoint, out Vector2 hitNormal, out float tOut,
        float intersectionEpsilon = DefaultIntersectionEpsilon)
    {
        hitPoint = Vector2.zero;
        hitNormal = Vector2.zero;
        tOut = float.MaxValue;
        bool found = false;

        if (Mathf.Abs(dir.x) > intersectionEpsilon)
        {
            float tx = (minX - pos.x) / dir.x;
            if (tx > intersectionEpsilon)
            {
                Vector2 p = pos + dir * tx;
                if (p.y >= minY - intersectionEpsilon && p.y <= maxY + intersectionEpsilon && tx < tOut)
                {
                    tOut = tx;
                    hitPoint = p;
                    hitNormal = Vector2.right;
                    found = true;
                }
            }

            float tx2 = (maxX - pos.x) / dir.x;
            if (tx2 > intersectionEpsilon)
            {
                Vector2 p = pos + dir * tx2;
                if (p.y >= minY - intersectionEpsilon && p.y <= maxY + intersectionEpsilon && tx2 < tOut)
                {
                    tOut = tx2;
                    hitPoint = p;
                    hitNormal = Vector2.left;
                    found = true;
                }
            }
        }

        if (Mathf.Abs(dir.y) > intersectionEpsilon)
        {
            float ty = (minY - pos.y) / dir.y;
            if (ty > intersectionEpsilon)
            {
                Vector2 p = pos + dir * ty;
                if (p.x >= minX - intersectionEpsilon && p.x <= maxX + intersectionEpsilon && ty < tOut)
                {
                    tOut = ty;
                    hitPoint = p;
                    hitNormal = Vector2.up;
                    found = true;
                }
            }

            float ty2 = (maxY - pos.y) / dir.y;
            if (ty2 > intersectionEpsilon)
            {
                Vector2 p = pos + dir * ty2;
                if (p.x >= minX - intersectionEpsilon && p.x <= maxX + intersectionEpsilon && ty2 < tOut)
                {
                    tOut = ty2;
                    hitPoint = p;
                    hitNormal = Vector2.down;
                    found = true;
                }
            }
        }

        return found;
    }

    void DrawTrajectory()
    {
        if (ballTransform == null || playAreaCollider == null)
        {
            trajectoryLine.positionCount = 0;
            return;
        }

        var points = new List<Vector3>();
        var planeZ = ballTransform.position.z;
        var pos = GetBallWorldCenter();
        var radius = GetBallWorldRadius();

        var bounds = playAreaCollider.bounds;
        float inset = radius;
        float minX = bounds.min.x + inset,
            maxX = bounds.max.x - inset,
            minY = bounds.min.y + inset,
            maxY = bounds.max.y - inset;

        // initial direction (unit vector)
        Vector2 dir = launchVelocity.sqrMagnitude > DefaultVelocityEpsilon
            ? launchVelocity.normalized
            : EstimateDirectionFromCharge();
        if (dir.sqrMagnitude < 1e-6f) dir = Vector2.up;

        points.Add(new Vector3(pos.x, pos.y, planeZ));

        int bouncesLeft = Mathf.Max(0, trajectoryMaxBounces);
        int segments = 1;

        while (bouncesLeft >= 0 && segments < trajectoryMaxSegments)
        {
            // compute nearest wall intersection
            if (!ComputeNearestBoxIntersection(pos, dir, minX, maxX, minY, maxY, out var wallHit, out var wallNormal,
                    out var t))
            {
                var end = pos + dir * predictionStepLen;
                points.Add(new Vector3(end.x, end.y, planeZ));
                break;
            }

            // clamp
            wallHit.x = Mathf.Clamp(wallHit.x, minX, maxX);
            wallHit.y = Mathf.Clamp(wallHit.y, minY, maxY);

            // check for earliest brick along segment pos->wallHit using CircleCastAll and use hit.normal
            float segmentDist = Vector2.Distance(pos, wallHit);
            Vector2 castDir = (segmentDist > 0f) ? (wallHit - pos).normalized : dir;
            float castLen = segmentDist + 0.01f;
            Vector2 castOrigin = pos;

            var candidates = Physics2D.CircleCastAll(castOrigin, radius + collisionCheckExtra, castDir, castLen,
                brickLayerMask);
            RaycastHit2D earliestHit = new RaycastHit2D();
            float bestDist = float.MaxValue;
            foreach (var h in candidates)
            {
                if (h.collider == null) continue;
                if (h.distance > 0f && h.distance < bestDist)
                {
                    bestDist = h.distance;
                    earliestHit = h;
                }
            }

            if (earliestHit.collider != null)
            {
                var impact = earliestHit.point;
                var impactNormal = earliestHit.normal.sqrMagnitude > 1e-6f ? earliestHit.normal : Vector2.up;

                // record hit
                points.Add(new Vector3(impact.x, impact.y, planeZ));
                segments++;

                // if this is last bounce, stop at hit (no outgoing fragment)
                if (bouncesLeft <= 1) break;

                // reflect direction using impact normal
                dir = Vector2.Reflect(dir, impactNormal).normalized;

                // forward visualization (constant)
                Vector2 forwardPoint = impact + dir * Mathf.Max(predictionStepLen * 0.5f, radius * 0.5f);
                points.Add(new Vector3(forwardPoint.x, forwardPoint.y, planeZ));
                segments++;

                float nudge = Mathf.Max(radius * 2f, predictionStepLen);
                pos = impact + dir * (trajectorySkinDistance + nudge);

                bouncesLeft--;
                continue;
            }

            // no brick hit on the way, record wall hit
            points.Add(new Vector3(wallHit.x, wallHit.y, planeZ));
            segments++;

            // if this is last bounce, stop at wall
            if (bouncesLeft <= 1) break;

            dir = Vector2.Reflect(dir, wallNormal).normalized;

            Vector2 forwardWall = wallHit + dir * Mathf.Max(predictionStepLen * 0.5f, radius * 0.5f);
            points.Add(new Vector3(forwardWall.x, forwardWall.y, planeZ));
            segments++;

            pos = wallHit + dir * trajectorySkinDistance;
            bouncesLeft--;
        }

        trajectoryLine.positionCount = points.Count;
        for (var i = 0; i < points.Count; i++) trajectoryLine.SetPosition(i, points[i]);
    }
}