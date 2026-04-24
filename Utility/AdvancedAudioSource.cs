using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedAudioSource : MonoBehaviour
{
    public AudioSource source;
    bool isFadingIn, isFadingOut;
    float fadeSpeed, defaultVolume, targetVolume;
    public bool mute = false;

    void Awake()
    {
        if (!source) source = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (source) defaultVolume = source.volume;
    }

    void Update()
    {
        if (isFadingIn)
        {
            source.volume += fadeSpeed * Time.deltaTime;
            source.volume = Mathf.Min(source.volume, targetVolume);

            if (source.volume >= targetVolume)
                isFadingIn = false;
        }

        if (isFadingOut)
        {
            source.volume -= fadeSpeed * Time.deltaTime;
            source.volume = Mathf.Max(source.volume, 0f);

            if (source.volume <= 0)
            {
                source.Stop();
                isFadingOut = false;
                source.volume = defaultVolume;
            }
        }
    }

    public void SetVolume(float volume)
    {
        defaultVolume = Mathf.Min(0f, volume);
    }

    public void Play(AudioClip clip, float volume = 1f, bool randomize = false, float fadeInDuration = 0)
    {
        if (mute) return;

        if (clip) source.clip = clip; // change clip if needed

        if (volume < 0f) volume = defaultVolume; // if no input volume, use defaultVolume

        if (randomize) RandomizeAudio(volume);
        else source.volume = volume;

        source.Play();
        
        if (fadeInDuration > 0)
        {
            targetVolume = source.volume;
            fadeSpeed = targetVolume / fadeInDuration;
            isFadingIn = true;
            isFadingOut = false;
            source.volume = 0f;
        }
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f, bool randomize = false)
    {
        if (mute) return;

        if (volume < 0f) volume = defaultVolume;

        if (randomize) {
            source.pitch = Random.Range(0.9f, 1.1f);
            volume *= Random.Range(0.9f, 1f);
        }

        source.PlayOneShot(clip, volume);
    }

    public void Stop(float fadeOutDuration = 0)
    {
        if (mute) return;

        if (fadeOutDuration > 0)
        {
            targetVolume = source.volume;
            fadeSpeed = targetVolume / fadeOutDuration;
            isFadingOut = true;
            isFadingIn = false;
        }
        else source.Stop();
    }

    void RandomizeAudio(float volume = 1f)
    {
        source.pitch = Random.Range(0.9f, 1.1f);
        source.volume = Random.Range(0.9f, 1f) * volume;
    }
}
