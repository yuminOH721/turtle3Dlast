using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TurtleDrawer : MonoBehaviour
{
    public Transform penTip; // 선이 시작될 위치 (펜 위치)
    public float drawDistanceThreshold = 0.01f;

    public bool isDrawingEnabled = false;

    private LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();

    void Start()
    {
        ClearTrail();
    }

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        points = new List<Vector3>();
    }


    void Update()
    {
        if (!isDrawingEnabled)
            return;
        
          if (points.Count == 0 || 
            Vector3.Distance(penTip.position, points[points.Count - 1]) > drawDistanceThreshold)
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

    public void ClearTrail()
    {
        points.Clear();
        lineRenderer.positionCount = 0;
 
        if (penTip != null)
            AddPoint();
    }
    public void StartDrawing()
    {
        isDrawingEnabled = true;
        
    }

    public void StopDrawing()
    {
        isDrawingEnabled = false;
    }
}
