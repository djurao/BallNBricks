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
        if (trajectoryLine == null) trajectoryLine = GetComponent<LineRenderer>();
        if (trajectoryLine != null)
        {
            trajectoryLine.useWorldSpace = true;
            trajectoryLine.positionCount = 0;
        }
    }

    public void UpdateTrajectoryIfNeeded()
    {
        if (trajectoryLine == null) trajectoryLine = GetComponent<LineRenderer>();
        if (trajectoryLine == null) return;

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
        float planeZ = ballTransform.position.z;

        Vector2 ballCenter = GetBallWorldCenter();
        float radius = GetBallWorldRadius();

        Bounds b = playAreaCollider.bounds;
        float inset = radius;
        float minX = b.min.x + inset;
        float maxX = b.max.x - inset;
        float minY = b.min.y + inset;
        float maxY = b.max.y - inset;

        Vector2 initialVel = launchVelocity;
        if (initialVel.sqrMagnitude < 1e-8f) initialVel = EstimateVelocityFromCharge();

        points.Add(new Vector3(ballCenter.x, ballCenter.y, planeZ));

        Vector2 pos = ballCenter;
        Vector2 vel = initialVel;
        int bouncesLeft = Mathf.Max(0, trajectoryMaxBounces);
        int segments = 1;

        while (bouncesLeft >= 0 && segments < trajectoryMaxSegments)
        {
            if (vel.sqrMagnitude < 1e-8f) break;

            Vector2 dir = vel.normalized;
            if (!ComputeNearestBoxIntersection(pos, dir, minX, maxX, minY, maxY, out Vector2 hitPoint,
                    out Vector2 hitNormal, out float t))
            {
                Vector2 end = pos + dir * 5f;
                points.Add(new Vector3(end.x, end.y, planeZ));
                break;
            }

            hitPoint.x = Mathf.Clamp(hitPoint.x, minX, maxX);
            hitPoint.y = Mathf.Clamp(hitPoint.y, minY, maxY);

            points.Add(new Vector3(hitPoint.x, hitPoint.y, planeZ));
            segments++;

            float speed = vel.magnitude;
            Vector2 reflectedDir = Vector2.Reflect(vel.normalized, hitNormal);
            float rest = Mathf.Clamp01(restitution);
            vel = reflectedDir * speed * rest;

            pos = hitPoint + reflectedDir * trajectorySkinDistance;
            bouncesLeft--;
        }

        trajectoryLine.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++) trajectoryLine.SetPosition(i, points[i]);
    }

// Helper: ball center
    Vector2 GetBallWorldCenter()
    {
        CircleCollider2D cc = GetComponent<CircleCollider2D>();
        if (cc != null) return (Vector2)ballTransform.TransformPoint(cc.offset);
        return (Vector2)ballTransform.position;
    }

// Helper: ball radius
    float GetBallWorldRadius()
    {
        CircleCollider2D cc = GetComponent<CircleCollider2D>();
        if (cc != null)
        {
            float scale = Mathf.Max(Mathf.Abs(ballTransform.lossyScale.x), Mathf.Abs(ballTransform.lossyScale.y));
            return Mathf.Max(0.0001f, cc.radius * scale);
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
        out Vector2 hitPoint, out Vector2 hitNormal, out float tOut)
    {
        hitPoint = Vector2.zero;
        hitNormal = Vector2.zero;
        tOut = float.MaxValue;
        bool found = false;

        if (Mathf.Abs(dir.x) > 1e-6f)
        {
            float tx = (minX - pos.x) / dir.x;
            if (tx > 1e-6f)
            {
                Vector2 p = pos + dir * tx;
                if (p.y >= minY - 1e-6f && p.y <= maxY + 1e-6f && tx < tOut)
                {
                    tOut = tx;
                    hitPoint = p;
                    hitNormal = Vector2.right;
                    found = true;
                }
            }

            float tx2 = (maxX - pos.x) / dir.x;
            if (tx2 > 1e-6f)
            {
                Vector2 p = pos + dir * tx2;
                if (p.y >= minY - 1e-6f && p.y <= maxY + 1e-6f && tx2 < tOut)
                {
                    tOut = tx2;
                    hitPoint = p;
                    hitNormal = Vector2.left;
                    found = true;
                }
            }
        }

        if (Mathf.Abs(dir.y) > 1e-6f)
        {
            float ty = (minY - pos.y) / dir.y;
            if (ty > 1e-6f)
            {
                Vector2 p = pos + dir * ty;
                if (p.x >= minX - 1e-6f && p.x <= maxX + 1e-6f && ty < tOut)
                {
                    tOut = ty;
                    hitPoint = p;
                    hitNormal = Vector2.up;
                    found = true;
                }
            }

            float ty2 = (maxY - pos.y) / dir.y;
            if (ty2 > 1e-6f)
            {
                Vector2 p = pos + dir * ty2;
                if (p.x >= minX - 1e-6f && p.x <= maxX + 1e-6f && ty2 < tOut)
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