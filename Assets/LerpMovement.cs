using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Rendering.HableCurve;

public class LerpMovement : MonoBehaviour
{
    public Transform ball;
    public List<Transform> points;
    public List<Transform> centers;
    public Transform center;
    public float angleStep;
    public List<float> offsetValue;
    public List<float> lastValue;
    public List<Vector3> lastPos;
    public List<bool> clockwise;
    public int step;

    float radius = 1f; // Radius of the circle
    int segments = 10000; // Number of segments for smoothness
    float lineWidth = 0.01f; // Width of the circle line

    float radiusOffset = 5f; // Radius of the circle

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        foreach (Transform t in points)
        {
            Transform cen = Instantiate(center);
            cen.SetParent(transform);
            centers.Add(cen);
            offsetValue.Add(0);
            lastValue.Add(0);

            Vector3 pos = t.position;
            lastPos.Add(pos);

            clockwise.Add(false);
            LineRenderer angleRenderer = cen.GetComponent<LineRenderer>();
            LineRenderer fullLineRenderer = cen.GetChild(0).GetComponent<LineRenderer>();
            angleRenderer.useWorldSpace = true; // Keep the circle local to the GameObject
            fullLineRenderer.useWorldSpace = false; // Keep the circle local to the GameObject
            angleRenderer.startWidth = lineWidth * 2;
            fullLineRenderer.startWidth = lineWidth;
        }
        ball.position = points[0].position;

    }

    private void DrawFullCircle(Transform center)
    {
        LineRenderer fullLineRenderer = center.GetChild(0).GetComponent<LineRenderer>();
        fullLineRenderer.positionCount = segments + 1; // Total points = segments + 1 to close the circle

        float angleStep = 360f / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 point = new Vector3(Mathf.Cos(angle) * radius * radiusOffset, Mathf.Sin(angle) * radius * radiusOffset, 0);
            fullLineRenderer.SetPosition(i, point);
        }
    }

    public void AdjustCenter()
    {
        for (int i = 0; i < points.Count; i++)
        {
            int begin = i % points.Count;
            int finish = (i + 1) % points.Count;

            Vector3 midpoint = (points[begin].localPosition + points[finish].localPosition) / 2;
            Vector3 direction = (points[finish].localPosition - points[begin].localPosition).normalized;
            Vector3 offsetDirection = Vector3.Cross(direction, Vector3.forward).normalized;

            centers[begin].localPosition = midpoint + offsetDirection * offsetValue[begin];
            radius = Vector2.Distance(centers[begin].localPosition, points[begin].localPosition);

            DrawFullCircle(centers[begin]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        AdjustCenter();

        int start = step % points.Count;
        Vector2 a = ball.localPosition;
        Vector2 c = centers[start].localPosition;

        radius = Vector2.Distance(centers[start].localPosition, points[start].localPosition);
        float adjustAngle = angleStep * radius / 0.5f;

        float newX = Mathf.Cos(adjustAngle) * (a.x - c.x) - Mathf.Sin(adjustAngle) * (a.y - c.y) + c.x;
        float newY = Mathf.Sin(adjustAngle) * (a.x - c.x) + Mathf.Cos(adjustAngle) * (a.y - c.y) + c.y;

        ball.localPosition = new Vector3(newX, newY);

        if (Vector2.Distance(ball.localPosition, points[(step + 1) % points.Count].position) < 0.1f * adjustAngle * 10)
        {
            ball.localPosition = points[(step + 1) % points.Count].position;
            step++;
            foreach (Transform cen in centers)
            {
                LineRenderer angleRenderer = cen.GetComponent<LineRenderer>();
                LineRenderer lineRenderer = cen.GetChild(1).GetComponent<LineRenderer>();
                LineRenderer insideAngleRenderer = cen.GetChild(2).GetComponent<LineRenderer>();
                Color setColor= Color.white;

                if (centers[step % points.Count] == cen)
                {
                    setColor = Color.red;
                }

                angleRenderer.startColor = setColor;
                angleRenderer.endColor = setColor;
                lineRenderer.startColor = setColor;
                lineRenderer.endColor = setColor;
                insideAngleRenderer.startColor = setColor;
                insideAngleRenderer.endColor = setColor;

            }
        }
    }
}
