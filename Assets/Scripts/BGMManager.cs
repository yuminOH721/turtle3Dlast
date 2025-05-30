using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BGMManager : MonoBehaviour
{
    public AudioSource bgmAudioSource;
    public Slider volumeSlider;

    private void Start()
    {
        bgmAudioSource = GetComponent<AudioSource>();
        if (volumeSlider != null)
        {
            volumeSlider.value = bgmAudioSource.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void SetVolume(float volume)
    {
        bgmAudioSource.volume = volume;
    }
}