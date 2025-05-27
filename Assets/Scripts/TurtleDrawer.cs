using System.Collections.Generic;
using UnityEngine;

//================================================================================
// TurtleDrawer: Trail 그리기 및 펜 설정
//================================================================================
public class TurtleDrawer : MonoBehaviour
{
    public Transform penTip;
    public float drawDistanceThreshold = 0.1f;

    private Transform gridcube;
    private bool isDrawingEnabled;

    private List<Vector3> localPoints = new();
    private LineRenderer currentLine;
    private Material lineMaterial;
    private float lineStartWidth, lineEndWidth;

    void Awake()
    {
        var baseLR = GetComponent<LineRenderer>();
        lineMaterial = baseLR.material;
        lineStartWidth = baseLR.startWidth;
        lineEndWidth = baseLR.endWidth;
        baseLR.enabled = false;
        var found = GameObject.Find("Gridcube");
        gridcube = found != null ? found.transform : null;
    }

    public void StartDrawing()
    {
        var go = new GameObject("TurtleTrail");
        go.transform.SetParent(gridcube, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = lineMaterial;
        lr.startWidth = lineStartWidth;
        lr.endWidth = lineEndWidth;
        currentLine = lr;
        localPoints.Clear();
        isDrawingEnabled = true;
    }

    public void StopDrawing()
    {
        isDrawingEnabled = false;
        currentLine = null;
        localPoints.Clear();
    }

    public void ClearAllTrails()
    {
        var trails = Object.FindObjectsByType<LineRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var lr in trails)
        {
            if (lr.gameObject.name == "TurtleTrail")
                Destroy(lr.gameObject);
        }
    }

    public void SetPenColor(Color c)
    {
        if (currentLine != null)
            currentLine.material.color = c;
    }

    public void SetPenSize(float s)
    {
        if (currentLine != null)
            currentLine.startWidth = currentLine.endWidth = s;
    }

    void Update()
    {
        if (!isDrawingEnabled || penTip == null || gridcube == null || currentLine == null)
            return;
        var pt = gridcube.InverseTransformPoint(penTip.position);
        if (localPoints.Count == 0 || Vector3.Distance(pt, localPoints[^1]) > drawDistanceThreshold)
        {
            localPoints.Add(pt);
            currentLine.positionCount = localPoints.Count;
            for (int i = 0; i < localPoints.Count; i++)
                currentLine.SetPosition(i, gridcube.TransformPoint(localPoints[i]));
        }
    }
}
