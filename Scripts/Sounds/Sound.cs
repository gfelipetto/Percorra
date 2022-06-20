using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;

    [Range(0f, 1f)]
    public float spatialBlend;

    public float minDistance;
    public float maxDistance;

    public AudioMixerGroup mixerOutput;
    public AudioRolloffMode rollofMode;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
}
