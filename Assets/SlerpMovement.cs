using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

[RequireComponent(typeof(LineRenderer))]
public class SlerpMovement : MonoBehaviour
{
    public List<Transform> startPoint;
    public List<Transform> centers;
    public Transform center;

    public int step;
    public Transform test;
    public Transform sphere;
    public Transform compar;
    public bool l = true;
    public bool r = true;
    public bool s = true;
    public int pathSegments = 10000; // Number of segments for half the path
    public List<LineRenderer> linesRenderer;

    public float interpolationFactor = 0f;
    float radiusOffset = 5f; // Radius of the circle

    void Start()
    {
        // Initialize the LineRenderer
        foreach (Transform t in startPoint)
        {
            LineRenderer renderer = t.GetComponent<LineRenderer>();
            renderer.useWorldSpace = true;

            linesRenderer.Add(renderer);
        }
        for (int i = 0; i < startPoint.Count - 1; i++)
        {
            Transform cen = Instantiate(center);
            cen.SetParent(transform);
            centers.Add(cen);

            Vector3 pos = centers[i].position;

            LineRenderer angleRenderer = cen.GetComponent<LineRenderer>();
            angleRenderer.useWorldSpace = true; // Keep the circle local to the GameObject

        }
        // Precompute the full path and draw it
    }

    private void Update()
    {
        if (s)
        {
            test.position = Slerp(startPoint[step % startPoint.Count].position, startPoint[(step + 1) % startPoint.Count].position, interpolationFactor);
            ComputeFullPath();
        }
        else
        {
            for (int q = 0; q < startPoint.Count - 1; q++)
            {
                linesRenderer[q].positionCount = 0;
            }
            test.position = startPoint[0].position;
        }
        if (r)
        {
            sphere.position = Rotate(startPoint[step % startPoint.Count].position, startPoint[(step + 1) % startPoint.Count].position, interpolationFactor);
            AdjustCenter();
        }
        else
        {
            foreach (Transform c in centers)
            {
                c.GetChild(0).GetComponent<LineRenderer>().positionCount = 0;
            }
            sphere.position = startPoint[0].position;
        }
        if (l)
        {
            DrawLine();
            compar.position = Lerp(startPoint[step % startPoint.Count].position, startPoint[(step + 1) % startPoint.Count].position, interpolationFactor);
        }
        else
        {
            compar.GetComponent<LineRenderer>().positionCount = 0;
            compar.position = startPoint[0].position;
        }
    }
    public void DrawLine()
    {
        LineRenderer lineRenderer = compar.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPoint[step % startPoint.Count].position);
        lineRenderer.SetPosition(1, startPoint[(step+1) % startPoint.Count].position);
    }
    public static Vector3 Rotate(Vector3 from, Vector3 to, float t)
    {
        // Calculate the center of the circle
        Vector3 center = (from + to) / 2.0f;

        // Calculate the radius of the circle
        float radius = Vector3.Distance(from, center);

        // Calculate the direction vectors from the center to the 'from' and 'to' positions
        Vector3 fromDirection = (from - center).normalized;
        Vector3 toDirection = (to - center).normalized;

        // Calculate the start and end angles (in radians)
        float startAngle = Mathf.Atan2(fromDirection.y, fromDirection.x); // Start angle
        float endAngle = Mathf.Atan2(toDirection.y, toDirection.x);       // End angle

        // Ensure clockwise rotation: If startAngle > endAngle, no adjustment is needed.
        // If startAngle <= endAngle, subtract 2π from endAngle to move clockwise.
        if (startAngle <= endAngle)
        {
            startAngle += 2 * Mathf.PI;
        }

        // Interpolate the angle based on t
        float currentAngle = Mathf.Lerp(startAngle, endAngle, t);

        // Calculate the new position on the circle
        float x = Mathf.Cos(currentAngle) * radius;
        float y = Mathf.Sin(currentAngle) * radius;

        // Convert local circle position to world space
        Vector3 newPosition = center + new Vector3(x, y, 0);

        return newPosition;
    }

    public void AdjustCenter()
    {
        for (int i = 0; i < centers.Count; i++)
        {
            Vector3 midpoint = (startPoint[i].localPosition + startPoint[i+1].localPosition) / 2;

            centers[i].localPosition = midpoint;

            float radius = Vector2.Distance(centers[i].localPosition, startPoint[i].localPosition);
            DrawCircle(startPoint[i], startPoint[i+1], centers[i], radius, true);
        }
    }

    private bool DrawCircle(Transform pointA, Transform pointB, Transform center, float radius, bool directinon)
    {
        LineRenderer lineRenderer = center.GetChild(0).GetComponentInChildren<LineRenderer>();

        if (directinon)
        {
            Transform temp = pointA;
            pointA = pointB;
            pointB = temp;
        }

        // Calculate the angles for pointA and pointB
        float angleA = Mathf.Atan2(pointA.localPosition.y - center.localPosition.y, pointA.localPosition.x - center.localPosition.x) * Mathf.Rad2Deg;
        float angleB = Mathf.Atan2(pointB.localPosition.y - center.localPosition.y, pointB.localPosition.x - center.localPosition.x) * Mathf.Rad2Deg;

        if (angleA < 0) angleA += 360;
        if (angleB < 0) angleB += 360;

        // Ensure angleA is the smaller angle
        if (angleA > angleB)
        {
            angleB += 360;
        }

        // Calculate the angular range to draw
        float angleRange = angleB - angleA;
        int segmentCount = Mathf.CeilToInt(pathSegments * (angleRange / 360f));

        // Update LineRenderer to only draw the segment
        lineRenderer.positionCount = segmentCount + 1;

        float angleStep = angleRange / segmentCount;

        for (int i = 0; i <= segmentCount; i++)
        {
            float angle = (angleA + i * angleStep) * Mathf.Deg2Rad;
            Vector3 point = new Vector3(Mathf.Cos(angle) * radius * radiusOffset, Mathf.Sin(angle) * radius * radiusOffset, 0);
            lineRenderer.SetPosition(i, point);
        }
        return true;
    }

    private void ComputeFullPath()
    {

        float angleStep = 720 / pathSegments;
        for (int q = 0; q < startPoint.Count-1; q++)
        {
            linesRenderer[q].positionCount = pathSegments +1;

            UnityEngine.Color newColor = UnityEngine.Color.white;
            if (q == step % startPoint.Count)
                newColor = UnityEngine.Color.red;

            linesRenderer[q].startColor = newColor;
            linesRenderer[q].endColor = newColor;

            for (int i = 0; i <= pathSegments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 point = Slerp(startPoint[q % startPoint.Count].position, startPoint[(q + 1) % startPoint.Count].position, angle);
                linesRenderer[q].SetPosition(i, point);
            }
        }
    }

    public static Vector3 Slerp(Vector3 from, Vector3 to, float t)
    {
        t = Mathf.Clamp(t, 0, 1);

        // v1' = v1 / |v1|, v2' = v2 / |v2|
        Vector3 fromNorm = Normalize(from);
        Vector3 toNorm = Normalize(to);

        // cos(theta) = v1' . v2'
        float cosTheta = (fromNorm.x * toNorm.x) + (fromNorm.y * toNorm.y) + (fromNorm.z * toNorm.z);

        // cos(theta) = Clamp(cos(theta), -1.0, 1.0)
        if (cosTheta > 1.0f) cosTheta = 1.0f;
        if (cosTheta < -1.0f) cosTheta = -1.0f;

        // theta = acos(cos(theta))
        float theta = (float)Mathf.Acos(cosTheta);

        if (theta < 0.0001f)
        {
            return Lerp(from, to, t);
        }

        // Formula: sin(theta) = sqrt(1 - cos^2(theta))
        float sinTheta = Mathf.Sqrt(1.0f - cosTheta * cosTheta);

        // scaleFrom = sin((1 - t) * theta) / sin(theta)
        float scaleFrom = (float)Mathf.Sin((1 - t) * theta) / sinTheta;
        //  scaleTo = sin(t * theta) / sin(theta)
        float scaleTo = (float)Mathf.Sin(t * theta) / sinTheta;

        // Slerp(v1, v2, t) = scaleFrom * v1 + scaleTo * v2
        return new Vector3(
            scaleFrom * from.x + scaleTo * to.x,
            scaleFrom * from.y + scaleTo * to.y
        );
    }
    private static Vector3 Normalize(Vector3 v)
    {
            float length = Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);

            //  v' = v / |v|
            if (Mathf.Approximately(length, 0f))
            {
                return Vector3.zero; // Return zero vector if length is nearly zero
            }
            return new Vector3(v.x / length, v.y / length, v.z / length);
        }

        private static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t,
                a.z + (b.z - a.z) * t
            );
        }
    }
