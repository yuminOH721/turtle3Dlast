using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TurtleDrawer : MonoBehaviour
{
    public Transform penTip; // 선이 시작될 위치 (펜 위치)
    public float drawDistanceThreshold = 0.01f;

    private LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        AddPoint(); // 시작점 추가
    }

    void Update()
    {
        if (Vector3.Distance(penTip.position, points[points.Count - 1]) > drawDistanceThreshold)
        {
            AddPoint();
        }
    }

    void AddPoint()
    {
        points.Add(penTip.position);
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
}
