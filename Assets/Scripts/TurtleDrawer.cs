using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    private Color pendingColor = default;

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

        pendingColor = default;
        StartDrawing();
    }

    public void StartDrawing()
    {
        if (isDrawingEnabled) return;

        var go = new GameObject("TurtleTrail");
        go.transform.SetParent(gridcube, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;

        // ── 머티리얼/셰이더 설정 ────────────────────────────────────────
        Material mat;
        if (pendingColor != default)
        {
            // 단색 모드: Unlit/Color 셰이더 사용
            var shader = Shader.Find("Unlit/Color");
            mat = new Material(shader);
            mat.color = pendingColor;
        }
        else
        {
            // 무지개 모드: vertex color 그라디언트 지원 셰이더
            // (Sprite Default 는 버텍스 컬러를 지원합니다)
            var shader = Shader.Find("Sprites/Default");
            mat = new Material(shader);
            // gradient 는 아래 ApplyRainbow()에서 세팅
        }
        lr.material = mat;
        // ────────────────────────────────────────────────────────────────

        lr.startWidth = lineStartWidth;
        lr.endWidth = lineEndWidth;

        currentLine = lr;
        isDrawingEnabled = true;
        localPoints.Clear();

        // 무지개 모드일 때만 그라데이션 적용
        if (pendingColor == default)
            ApplyRainbow();

        // 초기 포인트
        if (penTip != null)
        {
            var pt = gridcube.InverseTransformPoint(penTip.position);
            localPoints.Add(pt);
            currentLine.positionCount = 1;
            currentLine.SetPosition(0, pt);
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
        pendingColor = default;

        var trails = Object.FindObjectsByType<LineRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var lr in trails)
            if (lr.gameObject.name == "TurtleTrail")
                Destroy(lr.gameObject);
    }

    public void SetPenColor(Color c)
    {
        if (isDrawingEnabled && localPoints.Count <= 1)
        {
            currentLine.material.color = c;
            pendingColor = c;  // 다음 StartDrawing 때도 이 색 사용
            return;
        }

        // 이미 그리는 중이고, 실제 궤적이 있으면 → 새 LineRenderer 생성
        if (isDrawingEnabled)
            StopDrawing();

        pendingColor = c;
        StartDrawing();
    }

    public void ResetToRainbow()
    {
        pendingColor = default;       // 무지개 모드 신호

        if (isDrawingEnabled)
            StopDrawing();           // 기존 트레일 끝맺기

        StartDrawing();              // 무지개로 StartDrawing() 분기
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


    public LineRenderer[] GetAllTrails()
{
    // gridcube에 붙은 모든 LineRenderer 중 이름이 "TurtleTrail"인 것만 반환
    return gridcube
        .GetComponentsInChildren<LineRenderer>(false)
        .Where(lr => lr.gameObject.name == "TurtleTrail")
        .ToArray();
}


}