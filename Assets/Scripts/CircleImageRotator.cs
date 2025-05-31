using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleImageRotator : MonoBehaviour
{
    public GameObject imagePrefab;      // 회전할 이미지 프리팹
    public int imageCount = 20;          // 이미지 개수
    public float radius = 4f;           // 반지름
    public float rotationSpeed = 20f;   // 회전 속도
    public Transform userTransform;

    private List<GameObject> images = new List<GameObject>();

    void Start()
    {
        // 원형 배치
        for (int i = 0; i < imageCount; i++)
        {
            float angle = i * Mathf.PI * 2f / imageCount;
            Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            GameObject img = Instantiate(imagePrefab, transform.position + pos, Quaternion.identity, transform);
            images.Add(img);
        }
    }

    void Update()
    {
        // 전체 원 회전
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 각 이미지가 사용자 쪽을 바라보도록
        foreach (var img in images)
        {
            // 카메라를 바라보도록 설정
            img.transform.LookAt(userTransform);

            // 이미지가 정면을 향하도록 180도 회전 추가 (필요에 따라 X,Y,Z 조정)
            img.transform.Rotate(0, 180, 0);
        }

    }
}
