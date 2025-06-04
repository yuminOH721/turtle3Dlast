using UnityEngine;
using UnityEngine.UI;

public class HandScrollbarController : MonoBehaviour
{
    public Scrollbar scrollbar;

    private Transform fingerTransform;
    private bool isTouching = false;

    private float minY, maxY;

    void Start()
    {
        // 핸들에 Collider가 없으면 추가
        if (!scrollbar.handleRect.GetComponent<Collider>())
        {
            BoxCollider col = scrollbar.handleRect.gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
        }

        // Scrollbar 전체 영역에 Collider가 없다면 생성
        if (!GetComponent<Collider>())
        {
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
        }

        // 높이 범위 계산
        var bounds = GetComponent<Collider>().bounds;
        minY = bounds.min.y;
        maxY = bounds.max.y;
    }

    void Update()
    {
        if (isTouching && fingerTransform != null)
        {
            float fy = fingerTransform.position.y;
            float t = Mathf.InverseLerp(minY, maxY, fy);
            scrollbar.value = Mathf.Clamp01(t);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            fingerTransform = other.transform;
            isTouching = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            isTouching = false;
            fingerTransform = null;
        }
    }
}
