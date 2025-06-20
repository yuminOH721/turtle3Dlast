// File: LineValidator.cs
// Place this in Assets/Scripts/Validators (or wherever you keep utility classes)

using UnityEngine;

public static class LineValidator
{
    /// <summary>
    /// 순서 무관, 위치 오차 허용 비교
    /// </summary>
    public static bool AreShapesEquivalentIgnoreOrder(
        Vector3[] a,
        Vector3[] b,
        float posTol = 0.001f)
    {
        if (a.Length != b.Length) return false;
        bool[] used = new bool[a.Length];

        for (int i = 0; i < b.Length; i++)
        {
            bool found = false;
            for (int j = 0; j < a.Length; j++)
            {
                if (used[j]) continue;
                if (Vector3.SqrMagnitude(a[j] - b[i]) <= posTol * posTol)
                {
                    used[j] = true;
                    found = true;
                    break;
                }
            }
            if (!found) return false;
        }
        return true;
    }

    /// <summary>
    /// 단일 색 오차 허용 비교
    /// </summary>
    public static bool ColorsClose(
        Color c1,
        Color c2,
        float tol = 0.01f)
    {
        return Mathf.Abs(c1.r - c2.r) <= tol &&
               Mathf.Abs(c1.g - c2.g) <= tol &&
               Mathf.Abs(c1.b - c2.b) <= tol &&
               Mathf.Abs(c1.a - c2.a) <= tol;
    }

    /// <summary>
    /// LineRenderer의 좌표와 단일 색이 정답과 일치하는지 검사
    /// </summary>
    /// <param name="lr">검사할 LineRenderer</param>
    /// <param name="correctPositions">정답 좌표 배열</param>
    /// <param name="expectedColor">정답 색상</param>
    /// <param name="posTol">좌표 오차 허용</param>
    /// <param name="colorTol">색상 오차 허용</param>
    public static bool IsLineCorrect(
        LineRenderer lr,
        Vector3[] correctPositions,
        Color expectedColor,
        float posTol = 0.001f,
        float colorTol = 0.01f)
    {
        // 1) 좌표 비교
        var userPos = new Vector3[lr.positionCount];
        lr.GetPositions(userPos);
        if (!AreShapesEquivalentIgnoreOrder(userPos, correctPositions, posTol))
            return false;

        // 2) 단일 색 비교
        return ColorsClose(lr.material.color, expectedColor, colorTol);
    }
}
