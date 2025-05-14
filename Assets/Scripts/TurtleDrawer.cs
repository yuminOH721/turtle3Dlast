using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TurtleDrawer : MonoBehaviour
{
    public Transform penTip;
    public float drawDistanceThreshold = 0.01f;
    public bool isDrawingEnabled = false;

    private LineRenderer lineRenderer;
    private List<Vector3> localPoints = new();  // Gridcube 기준 로컬 좌표 저장

    private Transform gridcube;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;

        // Gridcube 자동 할당
        GameObject found = GameObject.Find("Gridcube");
        if (found != null)
        {
            gridcube = found.transform;
        }
        else
        {
            Debug.LogWarning("⚠️ 'Gridcube' 오브젝트를 찾지 못했습니다.");
        }
    }

    void Start()
    {
        ClearTrail();
    }

    void Update()
    {
        if (penTip == null || gridcube == null)
            return;

        // 선을 그리고 있는 경우
        if (isDrawingEnabled)
        {
            Vector3 localToGrid = gridcube.InverseTransformPoint(penTip.position);

            if (localPoints.Count == 0 ||
                Vector3.Distance(localToGrid, localPoints[localPoints.Count - 1]) > drawDistanceThreshold)
            {
                localPoints.Add(localToGrid);
            }
        }

        // Gridcube가 움직이거나 회전해도, 매 프레임 위치를 갱신해 줌
        UpdateLineRenderer();
    }

    void UpdateLineRenderer()
    {
        if (localPoints.Count == 0 || gridcube == null)
            return;

        Vector3[] worldPoints = new Vector3[localPoints.Count];
        for (int i = 0; i < localPoints.Count; i++)
        {
            worldPoints[i] = gridcube.TransformPoint(localPoints[i]);  // 매 프레임 새로 변환
        }

        lineRenderer.positionCount = worldPoints.Length;
        lineRenderer.SetPositions(worldPoints);
    }

    public void ClearTrail()
    {
        localPoints.Clear();
        lineRenderer.positionCount = 0;

        if (penTip != null && gridcube != null)
        {
            localPoints.Add(gridcube.InverseTransformPoint(penTip.position));
            UpdateLineRenderer();
        }
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
