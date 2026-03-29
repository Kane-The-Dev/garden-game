using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] Slider mainSlider;
    [SerializeField] Slider musicSlider, ambientSlider, SFXSlider;
    [SerializeField] TextMeshProUGUI mainValue, musicValue, ambientValue, SFXValue;

    [Header("Volume")]
    [Range(0f, 1f)] public float mainVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float ambientVolume = 1f;
    [Range(0f, 1f)] public float SFXVolume = 1f;

    // PlayerPrefs Keys
    const string MAIN_KEY = "MAIN_VOLUME";
    const string MUSIC_KEY = "MUSIC_VOLUME";
    const string AMBIENT_KEY = "AMBIENT_VOLUME";
    const string SFX_KEY = "SFX_VOLUME";

    AudioManager am;

    void Start()
    {
        am = AudioManager.instance;
        mainVolume = PlayerPrefs.GetFloat(MAIN_KEY, 1f);
        musicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        ambientVolume = PlayerPrefs.GetFloat(AMBIENT_KEY, 1f);
        SFXVolume = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        if (mainSlider) mainSlider.value = mainVolume;
        if (musicSlider) musicSlider.value = musicVolume;
        if (ambientSlider) ambientSlider.value = ambientVolume;
        if (SFXSlider) SFXSlider.value = SFXVolume;

        ApplyVolumes();
    }

    public void ChangeMainVolume(float value)
    {
        mainVolume = value;
        PlayerPrefs.SetFloat(MAIN_KEY, value);
        ApplyVolumes();
    }

    public void ChangeMusicVolume(float value)
    {
        musicVolume = value;
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
        ApplyVolumes();
    }

    public void ChangeAmbientVolume(float value)
    {
        ambientVolume = value;
        PlayerPrefs.SetFloat(AMBIENT_KEY, value);
        ApplyVolumes();
    }

    public void ChangeSFXVolume(float value)
    {
        SFXVolume = value;
        PlayerPrefs.SetFloat(SFX_KEY, value);
        ApplyVolumes();
    }

    void ApplyVolumes()
    {
        am.musicVolume = mainVolume * musicVolume;
        am.ambientVolume = mainVolume * ambientVolume;
        am.SFXVolume = mainVolume * SFXVolume;

        if (mainValue) mainValue.text = mainVolume.ToString("F1");
        if (musicValue) musicValue.text = musicVolume.ToString("F1");
        if (ambientValue) ambientValue.text = ambientVolume.ToString("F1");
        if (SFXValue) SFXValue.text = SFXVolume.ToString("F1");

        am.ApplyVolumes();
    }
}