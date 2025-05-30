using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spin : MonoBehaviour
{
    public float rotationSpeed = 360f; // 1초에 한바퀴
    public float spinDuration = 2f; // 회전 시간

    private float elapsed = 0f;
    private bool spinning = false;

    void Update()
    {
        if (spinning)
        {
            if (elapsed < spinDuration)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                elapsed += Time.deltaTime;
            }
            else
            {
                spinning = false; // 멈추기
            }
        }
    }

    public void StartSpin()
    {
        spinning = true;
        elapsed = 0f;
    }
}
