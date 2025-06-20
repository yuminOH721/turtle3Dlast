using System.Collections;
using System.Collections.Generic;    // ← 추가  
using UnityEngine;

//================================================================================
// Turtle3D: 이동/회전 로직 + 키 포인트 기록
//================================================================================
public class Turtle3D : MonoBehaviour
{
    public Transform tr;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private float gridScale = 1f;
    public Transform gridParent;
    public string TurtleName { get; private set; }

    // ─── 키 위치 기록용 리스트 ───────────────────────────────
    private List<Vector3> keyPositions = new List<Vector3>();

    void Awake()
    {
        tr = transform;
        if (gridParent == null && tr.parent != null)
            gridParent = tr.parent;
    }

    public void Initialize(string name, Vector3 pos, Quaternion rot)
    {
        TurtleName = name;
        gameObject.name = name;
        if (gridParent != null)
        {
            tr.SetParent(gridParent, false);
            tr.localPosition = pos;
            tr.localRotation = rot;
        }
        else
        {
            tr.position = pos;
            tr.rotation = rot;
        }

        // ─── 초기화 시키면서 시작점 기록 ────────────────────────
        keyPositions.Clear();
        keyPositions.Add(tr.localPosition);
    }

    public Vector3 Position => tr.localPosition;

    public IEnumerator Forward(float units)
    {
        float dist = units * TurtleManager.instance.CellSize;
        Vector3 start = tr.localPosition;
        Vector3 dir = tr.localRotation * Vector3.forward;
        Vector3 end = start + dir * dist;

        Debug.Log($"Forward({units}) → dist={dist:F3}, from {start} to {end}");

        float duration = dist / moveSpeed;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            tr.localPosition = Vector3.Lerp(start, end, Mathf.Clamp01(t / duration));
            yield return null;
        }
        tr.localPosition = end;

        // ─── 이동 완료 후 끝점 기록 ─────────────────────────────
        keyPositions.Add(tr.localPosition);
    }

    public IEnumerator Rotate(float x, float y, float z)
    {
        Quaternion start = tr.rotation;
        Quaternion delta = Quaternion.Euler(x, y, z);
        Quaternion end = start * delta;
        float angle = Quaternion.Angle(start, end);
        float duration = angle / rotateSpeed;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            tr.rotation = Quaternion.Slerp(start, end, Mathf.Clamp01(t / duration));
            yield return null;
        }
        tr.rotation = end;
        // 회전만 기록 대상이 아니라면 기록 생략
    }

    public IEnumerator MoveTo(Vector3 localTarget)
    {
        Vector3 start = tr.localPosition;
        float dist = Vector3.Distance(start, localTarget);
        float duration = dist / moveSpeed;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            tr.localPosition = Vector3.Lerp(start, localTarget, Mathf.Clamp01(t / duration));
            yield return null;
        }
        tr.localPosition = localTarget;
    }


    // ─── 외부에서 키 포인트만 꺼내갈 수 있도록 Getter ─────────
    public Vector3[] GetKeyPositions() => keyPositions.ToArray();
}
