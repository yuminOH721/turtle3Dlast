using System.Collections;
using UnityEngine;

public class Turtle3D : MonoBehaviour
{
    private Transform tr;

    [Header("Speed Settings")]
    [Tooltip("이동 코루틴의 속도 계수 (클수록 빠름)")]
    [SerializeField] private float moveSpeed = 2f;
    [Tooltip("회전 코루틴의 속도 계수 (클수록 빠름)")]
    [SerializeField] private float rotateSpeed = 90f;

    [Header("Grid Settings")]
    [Tooltip("forward(1) 당 실제 이동 단위로 곱해줄 스케일")]
    [SerializeField] private float gridScale = 1f;
    [Tooltip("부모 그리드 Transform (할당 안되면 Awake 시 부모 사용)")]
    public Transform gridParent;



    public string TurtleName { get; private set; }

    void Awake()
    {
        tr = transform;
        if (gridParent == null && tr.parent != null)
            gridParent = tr.parent;
    }

    /// <summary>
    /// 이름과 초기 위치·회전을 설정
    /// </summary>
    public void Initialize(string name, Vector3 localPos, Quaternion localRot)
    {
        TurtleName = name;
        gameObject.name = name;

        if (gridParent != null)
        {
            tr.SetParent(gridParent, worldPositionStays: false);
            tr.localPosition = localPos;
            tr.localRotation = localRot;
        }
        else
        {
            tr.SetParent(null, worldPositionStays: false);
            tr.position = localPos;
            tr.rotation = localRot;
        }

    //    anim = GetComponent<Animation>();
    }


    public Vector3 Position => tr.position;

    /// <summary>
    /// 그리드 로컬 축 기준으로 부드럽게 전진 (회전된 방향 반영)
    /// </summary>
    public IEnumerator Forward(float units)
    {
        // 이동 방향을 거북이의 현재 로컬 Z축(전방)으로 계산
        float distance = units * gridScale * TurtleManager.instance.movementScale;
        Vector3 start = tr.localPosition;
        Vector3 direction = tr.localRotation * Vector3.forward;
        Vector3 end = start + direction * distance;
        float elapsed = 0f;
        float duration = distance / moveSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            tr.localPosition = Vector3.Lerp(start, end, t);
            yield return null;
        }
        tr.localPosition = end;
    }

    /// <summary>
    /// 그리드 로컬 축 기준으로 부드럽게 회전
    /// </summary>
    public IEnumerator Rotate(float x, float y, float z)
    {
        Quaternion start = tr.localRotation;
        Quaternion end = start * Quaternion.Euler(x, y, z);
        float elapsed = 0f;
        float angle = Quaternion.Angle(start, end);
        float duration = angle / rotateSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            tr.localRotation = Quaternion.Slerp(start, end, t);
            yield return null;
        }
        tr.localRotation = end;
   

       // anim.Play("Idle");
    }
}
