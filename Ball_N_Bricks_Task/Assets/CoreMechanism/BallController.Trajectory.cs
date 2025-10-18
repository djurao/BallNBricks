using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public partial class BallController
{
    [Header("Trajectory (Prediction)")] public LineRenderer trajectoryLine;
    public int trajectoryMaxBounces = 10;
    public int trajectoryMaxSegments = 200;
    public float trajectorySkinDistance = 0.02f;
    public bool trajectoryDebugLogs = false;
    
    void Awake()
    {
        trajectoryLine.useWorldSpace = true;
        trajectoryLine.positionCount = 0;
    }

    public void UpdateTrajectoryIfNeeded()
    {
        if (MouseHeld)
            DrawTrajectory();
        else
            trajectoryLine.positionCount = 0;
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

        var ballCenter = GetBallWorldCenter();
        var radius = GetBallWorldRadius();

        var b = playAreaCollider.bounds;
        var inset = radius;
        var minX = b.min.x + inset;
        var maxX = b.max.x - inset;
        var minY = b.min.y + inset;
        var maxY = b.max.y - inset;

        var initialVel = launchVelocity;
        if (initialVel.sqrMagnitude < 1e-8f) initialVel = EstimateVelocityFromCharge();

        points.Add(new Vector3(ballCenter.x, ballCenter.y, planeZ));

        var pos = ballCenter;
        var vel = initialVel;
        var bouncesLeft = Mathf.Max(0, trajectoryMaxBounces);
        var segments = 1;

        while (bouncesLeft >= 0 && segments < trajectoryMaxSegments)
        {
            if (vel.sqrMagnitude < 1e-8f) break;

            var dir = vel.normalized;
            if (!ComputeNearestBoxIntersection(pos, dir, minX, maxX, minY, maxY, out var hitPoint,
                    out var hitNormal, out var t, 1e-6f))
            {
                var end = pos + dir * 5f;
                points.Add(new Vector3(end.x, end.y, planeZ));
                break;
            }

            hitPoint.x = Mathf.Clamp(hitPoint.x, minX, maxX);
            hitPoint.y = Mathf.Clamp(hitPoint.y, minY, maxY);

            points.Add(new Vector3(hitPoint.x, hitPoint.y, planeZ));
            segments++;

            var speed = vel.magnitude;
            var reflectedDir = Vector2.Reflect(vel.normalized, hitNormal);
            var rest = Mathf.Clamp01(restitution);
            vel = reflectedDir * speed * rest;

            pos = hitPoint + reflectedDir * trajectorySkinDistance;
            bouncesLeft--;
        }

        trajectoryLine.positionCount = points.Count;
        for (var i = 0; i < points.Count; i++) trajectoryLine.SetPosition(i, points[i]);
    }

// Helper: ball center
    Vector2 GetBallWorldCenter()
    {
        if (ballCollider != null) return (Vector2)ballTransform.TransformPoint(ballCollider.offset);
        return (Vector2)ballTransform.position;
    }

// Helper: ball radius
    float GetBallWorldRadius()
    {
        if (ballCollider != null)
        {
            var scale = Mathf.Max(Mathf.Abs(ballTransform.lossyScale.x), Mathf.Abs(ballTransform.lossyScale.y));
            return Mathf.Max(0.0001f, ballCollider.radius * scale);
        }

        return 0.1f;
    }

// Helper: estimate velocity from charge
    Vector2 EstimateVelocityFromCharge()
    {
        // use launchForce and transform.up
        return ballTransform.up * launchForce;
    }

// Compute nearest intersection with axis-aligned box (min/max)
    bool ComputeNearestBoxIntersection(Vector2 pos, Vector2 dir, float minX, float maxX, float minY, float maxY,
        out Vector2 hitPoint, out Vector2 hitNormal, out float tOut, float intersectionEpsilon)
    {
        hitPoint = Vector2.zero;
        hitNormal = Vector2.zero;
        tOut = float.MaxValue;
        var found = false;

        if (Mathf.Abs(dir.x) > intersectionEpsilon)
        {
            var tx = (minX - pos.x) / dir.x;
            if (tx > intersectionEpsilon)
            {
                var p = pos + dir * tx;
                if (p.y >= minY - intersectionEpsilon && p.y <= maxY + intersectionEpsilon && tx < tOut)
                {
                    tOut = tx;
                    hitPoint = p;
                    hitNormal = Vector2.right;
                    found = true;
                }
            }

            var tx2 = (maxX - pos.x) / dir.x;
            if (tx2 > intersectionEpsilon)
            {
                var p = pos + dir * tx2;
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
            var ty = (minY - pos.y) / dir.y;
            if (ty > intersectionEpsilon)
            {
                var p = pos + dir * ty;
                if (p.x >= minX - intersectionEpsilon && p.x <= maxX + intersectionEpsilon && ty < tOut)
                {
                    tOut = ty;
                    hitPoint = p;
                    hitNormal = Vector2.up;
                    found = true;
                }
            }

            var ty2 = (maxY - pos.y) / dir.y;
            if (ty2 > intersectionEpsilon)
            {
                var p = pos + dir * ty2;
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
}