using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    public float rotationSpeed = 360f;
    public float spinDuration = 2f;

    private float elapsed = 0f;
    private bool spinning = false;
    private AudioClip spinClip;
    private AudioSource audioSource;

    private void Start()
    {
        // Resources에서 AudioClip 로드
        spinClip = Resources.Load<AudioClip>("Audio/SoundEffect/SpinSound");
        if (spinClip == null)
        {
            Debug.LogError("SpinSound 오디오를 찾을 수 없습니다. 경로를 확인하세요!");
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = spinClip;
        audioSource.loop = false;  // Loop는 코드로 처리
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (spinning)
        {
            if (elapsed < spinDuration)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                elapsed += Time.deltaTime;

                // Clip 끝나면 재생 (반복)
                if (!audioSource.isPlaying && spinClip != null)
                {
                    audioSource.Play();
                }
            }
            else
            {
                spinning = false;
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }
    }

    public void StartSpin()
    {
        spinning = true;
        elapsed = 0f;

        audioSource.pitch = 3.0f;
    }
}
