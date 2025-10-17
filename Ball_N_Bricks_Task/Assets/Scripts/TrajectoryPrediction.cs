using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPrediction2D : MonoBehaviour
{
    public BallController ballController;           // auto-find if null
    public LineRenderer lineRenderer;               // auto-get if null
    public int maxBounces = 5;
    public int maxSegments = 200;
    public LayerMask collisionMask = ~0;
    public float skinDistance = 0.02f;
    public float maxDistancePerCast = 50f;
    public Color debugCastColor = Color.yellow;
    public Color debugHitColor = Color.red;
    public Color debugNormalColor = Color.cyan;
    public bool runDebugTests = true;               // enable debug test outputs
    void Awake()
    {
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        if (ballController == null) return;
        if (Input.GetMouseButton(0))
            DrawPrediction();
        else
            lineRenderer.positionCount = 0;
    }

    void DrawPrediction()
    {
        List<Vector3> points = new List<Vector3>();
        Vector2 startPos2 = ballController.rb.position;
        Vector3 startPos = new Vector3(startPos2.x, startPos2.y, 0f);
        Vector2 dir2 = ballController.rb.transform.up;
        dir2.Normalize();
        float launchForce = ballController.launchForce;

        // initial velocity from impulse: v = impulse / mass
        float mass = Mathf.Max(1e-4f, ballController.rb.mass);
        Vector2 velocity2 = dir2 * (launchForce / mass);

        Vector2 gravityPerSecond = Physics2D.gravity * ballController.rb.gravityScale;

        // get ball radius from CircleCollider2D
        float radius = 0f;
        CircleCollider2D cc = ballController.GetComponent<CircleCollider2D>();
        if (cc != null) radius = cc.radius * Mathf.Max(ballController.rb.transform.lossyScale.x, ballController.rb.transform.lossyScale.y);
        if (radius <= 0f) radius = 0.1f; // fallback for testing

        points.Add(startPos);
        Vector2 currentPos2 = startPos2;
        Vector2 currentVel2 = velocity2;
        int bouncesLeft = maxBounces;
        int segments = 1;

        while (bouncesLeft >= 0 && segments < maxSegments)
        {
            if (currentVel2.sqrMagnitude < 1e-8f) break;
            float speed = currentVel2.magnitude;
            if (speed < 1e-6f) break;

            float castDistance = Mathf.Min(maxDistancePerCast, speed * 5f + 1f);
            Vector2 castDir = currentVel2.normalized;

            var allHits = Physics2D.CircleCastAll(currentPos2, radius, castDir, castDistance, collisionMask);
            RaycastHit2D chosen = new RaycastHit2D();
            float bestDist = float.MaxValue;
            foreach (var h in allHits)
            {
                if (h.collider == null) continue;
                if (h.collider.gameObject == ballController.gameObject) continue;
                if (h.collider.isTrigger) continue;
                if (h.distance > 0.0001f && h.distance < bestDist)
                {
                    bestDist = h.distance;
                    chosen = h;
                }
            }

            if (bestDist == float.MaxValue)
            {
                // no valid hit found: advance to endpoint
                Vector2 endPos2 = currentPos2 + castDir * castDistance;
                points.Add(new Vector3(endPos2.x, endPos2.y, 0f));
                segments++;

                float dt = castDistance / Mathf.Max(1e-6f, currentVel2.magnitude);
                currentVel2 += gravityPerSecond * dt;
                currentPos2 = endPos2;
                continue;
            }
            Vector2 hitPoint2 = chosen.point;
            points.Add(new Vector3(hitPoint2.x, hitPoint2.y, 0f));
            segments++;

            float distToHit = Vector2.Distance(currentPos2, hitPoint2);
            float dtHit = (currentVel2.magnitude > 1e-6f) ? distToHit / currentVel2.magnitude : 0f;
            Vector2 velAtImpact2 = currentVel2 + gravityPerSecond * dtHit;

            float bounciness = 1f;
            if (chosen.collider != null && chosen.collider.sharedMaterial != null) bounciness *= chosen.collider.sharedMaterial.bounciness;
            Collider2D ballCol = ballController.GetComponent<Collider2D>();
            if (ballCol != null && ballCol.sharedMaterial != null) bounciness *= ballCol.sharedMaterial.bounciness;

            Vector2 reflected2 = Vector2.Reflect(velAtImpact2, chosen.normal) * bounciness;

            currentPos2 = hitPoint2 + reflected2.normalized * skinDistance;
            currentVel2 = reflected2;

            bouncesLeft--;
        }

        lineRenderer.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++) lineRenderer.SetPosition(i, points[i]);
    }
}