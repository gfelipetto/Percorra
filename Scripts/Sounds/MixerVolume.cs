using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MixerVolume : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string parameter;

    public void SetVolume(float sliderValue)
    {
        mixer.SetFloat(parameter, Mathf.Log10(sliderValue) * 20);
    }
}
