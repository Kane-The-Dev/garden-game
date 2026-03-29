using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Clip
{
    public AudioClip sound;
    public float volume;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    [SerializeField] AudioSource ambient;
    [SerializeField] AdvancedAudioSource music, UISource;
    
    [SerializeField] AudioMixer musicMixer, SFXMixer;

    [Header("Clips")]
    [SerializeField] Clip[] UISounds;
    [SerializeField] Clip[] musicDiscs;

    [Header("Settings")]
    [Range(0f,1f)] public float musicVolume = 1f;
    [Range(0f,1f)] public float ambientVolume = 1f;
    [Range(0f,1f)] public float SFXVolume = 1f;

    [SerializeField] int nowPlaying = 0;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(ChangeSong());
    }

    IEnumerator ChangeSong()
    {
        while (true)
        {
            Clip nextSong = musicDiscs[(++nowPlaying) % musicDiscs.Length];
            music.Play(nextSong.sound, nextSong.volume);
            yield return new WaitWhile(() => music.source.isPlaying);
        }
    }

    public void PlayAmbient(bool loop = true)
    {
        ambient.Play();
    }

    public void ApplyVolumes()
    {
        float dB1 = Mathf.Log10(Mathf.Max(musicVolume, 0.001f)) * 20f;
        musicMixer.SetFloat("musicVolume", dB1);

        ambient.volume = ambientVolume;

        float dB2 = Mathf.Log10(Mathf.Max(SFXVolume, 0.001f)) * 20f;
        SFXMixer.SetFloat("SFXVolume", dB2);
    }

    public void PlayUISoundEffect(int clipID)
    {
        UISource.PlayOneShot(UISounds[clipID].sound, UISounds[clipID].volume, true);
    }
}
