using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] AudioSource music;
    [SerializeField] AudioSource ambient;
    [SerializeField] AudioSource button;
    [SerializeField] AudioSource sfxSource;

    [Header("Volume")]
    [Range(0f,1f)] public float masterVolume = 1f;
    [Range(0f,1f)] public float musicVolume = 1f;
    [Range(0f,1f)] public float sfxVolume = 1f;
    [Range(0f,1f)] public float uiVolume = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        
    }

    public void PlayMusic(bool loop = true)
    {
        music.Play();
        ambient.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFX(AudioClip clip, Vector3 position)
    {
        AudioSource.PlayClipAtPoint(clip, position, masterVolume * sfxVolume);
    }
}
