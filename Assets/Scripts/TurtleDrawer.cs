using System.Collections.Generic;
using UnityEngine;

//================================================================================
// TurtleDrawer: Trail 그리기 및 무지개 펜 설정
//================================================================================
public class TurtleDrawer : MonoBehaviour
{
    public Material rainbowLineMat;

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
        gridcube = TurtleManager.instance?.gridParent;
        if (gridcube == null)
            Debug.LogError("[TurtleDrawer] gridParent가 할당되지 않음.");

        var baseLR = GetComponent<LineRenderer>();
        lineMaterial = baseLR.material;
        lineStartWidth = baseLR.startWidth;
        lineEndWidth = baseLR.endWidth;
        baseLR.enabled = false;

        StartDrawing();
    }

    public void StartDrawing()
    {
        if (isDrawingEnabled) return;

        var go = new GameObject("TurtleTrail");
        go.transform.SetParent(gridcube, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;

        // 머티리얼 설정
        if (rainbowLineMat != null)
            lr.material = rainbowLineMat;
        else
            lr.material = lineMaterial;

        lr.startWidth = lineStartWidth;
        lr.endWidth = lineEndWidth;

        currentLine = lr;
        isDrawingEnabled = true;

        ApplyRainbow();

        // 추가됨 06.12
        if (penTip != null && gridcube != null)
        {
            var pt = gridcube.InverseTransformPoint(penTip.position);

            if (pt.y > -10f && pt.y < 10f) // 안전한 범위 안에 있을 때만
            {
                localPoints.Clear();
                localPoints.Add(pt);
                currentLine.positionCount = 1;
                currentLine.SetPosition(0, pt);
            }
        }
    }

    public void StopDrawing()
    {
        if (!isDrawingEnabled) return;

        isDrawingEnabled = false;
        currentLine = null;
        localPoints.Clear();
    }

    public void ClearAllTrails()
    {
        StopDrawing();
        var trails = Object.FindObjectsByType<LineRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var lr in trails)
            if (lr.gameObject.name == "TurtleTrail")
                Destroy(lr.gameObject);
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

    /// <summary>
    /// 현재 생성된 LineRenderer에 무지개 Gradient를 설정합니다.
    /// </summary>
    public void ApplyRainbow()
    {
        if (currentLine == null) return;

        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.red,    0.00f),
                new GradientColorKey(Color.yellow, 0.17f),
                new GradientColorKey(Color.green,  0.33f),
                new GradientColorKey(Color.cyan,   0.50f),
                new GradientColorKey(Color.blue,   0.67f),
                new GradientColorKey(new Color(0.5f, 0, 1f), 0.83f),
                new GradientColorKey(new Color(1f, 0, 1f),   1.00f),
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 1.0f),
            }
        );
        currentLine.colorGradient = gradient;
        currentLine.textureMode = LineTextureMode.Stretch;
    }

    // void Update()
    // {
    //     if (!isDrawingEnabled || penTip == null || gridcube == null || currentLine == null)
    //         return;

    //     var pt = gridcube.InverseTransformPoint(penTip.position);
    //     if (localPoints.Count == 0 || Vector3.Distance(pt, localPoints[^1]) > drawDistanceThreshold)
    //     {
    //         localPoints.Add(pt);
    //         currentLine.positionCount = localPoints.Count;
    //         for (int i = 0; i < localPoints.Count; i++)
    //             currentLine.SetPosition(i, gridcube.TransformPoint(localPoints[i]));
    //     }
    // }
    void Update()
    {
        if (!isDrawingEnabled || penTip == null || gridcube == null || currentLine == null)
            return;

        var pt = gridcube.InverseTransformPoint(penTip.position);  // 그리드 기준 로컬 좌표

        if (localPoints.Count == 0 || Vector3.Distance(pt, localPoints[^1]) > drawDistanceThreshold)
        {
            localPoints.Add(pt);
            currentLine.positionCount = localPoints.Count;
            for (int i = 0; i < localPoints.Count; i++)
                currentLine.SetPosition(i, localPoints[i]); // 로컬 좌표 그대로 사용
        }
    }

}