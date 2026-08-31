using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{ 
    [Header("Volume")]
    [SerializeField] Slider mainSlider;
    [SerializeField] Slider musicSlider, ambientSlider, SFXSlider, UISlider;
    [SerializeField] TextMeshProUGUI mainValue, musicValue, ambientValue, SFXValue, UIValue;
    [SerializeField] Toggle muteToggle;
    [Range(0f, 1f)] public float mainVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float ambientVolume = 1f;
    [Range(0f, 1f)] public float SFXVolume = 1f;
    [Range(0f, 1f)] public float UIVolume = 1f;
    public bool isMuted = false;

    [Header("Save And Load")]
    [SerializeField] Toggle autosaveToggle;
    [SerializeField] Slider autosaveIntervalSlider;
    [SerializeField] TextMeshProUGUI autosaveIntervalValue;
    [SerializeField] TMP_InputField saveFolderInput;
    [Range(0, 7)] public int autosaveIntervalID = 0;
    float[] autosaveIntervalValues = {60f, 120f, 180f, 300f, 600f, 900f, 1200f, 1500f};
    [SerializeField] Transform holder;
    [SerializeField] GameObject saveVersion;
    [SerializeField] TextMeshProUGUI pageIndicator;
    int currentPage, totalPage;
    List<KeyValuePair<string, GardenSaveData>> saveList = new List<KeyValuePair<string, GardenSaveData>>();

    // PlayerPrefs Keys
    const string MAIN_KEY = "MAIN_VOLUME";
    const string MUSIC_KEY = "MUSIC_VOLUME";
    const string AMBIENT_KEY = "AMBIENT_VOLUME";
    const string SFX_KEY = "SFX_VOLUME";
    const string UI_KEY = "UI_VOLUME";
    const string MUTE_KEY = "MUTE_ENABLED";
    const string AUTOSAVE_ENABLED_KEY = "AUTOSAVE_ENABLED";
    const string AUTOSAVE_INTERVAL_ID_KEY = "AUTOSAVE_INTERVAL_ID";
    const string SAVE_FOLDER_KEY = "SAVE_FOLDER";

    GameManager gm;
    AudioManager am;
    SaveAndLoad sal;

    void Start()
    {
        gm = GameManager.instance;
        am = AudioManager.instance;
        sal = FindObjectOfType<SaveAndLoad>();

        // Load Volume Settings
        mainVolume = PlayerPrefs.GetFloat(MAIN_KEY, 1f);
        musicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        ambientVolume = PlayerPrefs.GetFloat(AMBIENT_KEY, 1f);
        SFXVolume = PlayerPrefs.GetFloat(SFX_KEY, 1f);
        UIVolume = PlayerPrefs.GetFloat(UI_KEY, 1f);
        isMuted = PlayerPrefs.GetInt(MUTE_KEY, 0) == 1;

        if (mainSlider) mainSlider.value = mainVolume;
        if (musicSlider) musicSlider.value = musicVolume;
        if (ambientSlider) ambientSlider.value = ambientVolume;
        if (SFXSlider) SFXSlider.value = SFXVolume;
        if (UISlider) UISlider.value = UIVolume;
        if (muteToggle) muteToggle.isOn = isMuted;

        ApplyVolumes();

        // Load SaveAndLoad Settings
        bool autosaveEnabled = PlayerPrefs.GetInt(AUTOSAVE_ENABLED_KEY, 1) == 1;
        autosaveIntervalID = PlayerPrefs.GetInt(AUTOSAVE_INTERVAL_ID_KEY, 3); // defaults to 300s
        string defaultFolder = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Saves"));
        string saveFolder = PlayerPrefs.GetString(SAVE_FOLDER_KEY, defaultFolder);

        if (autosaveToggle) autosaveToggle.isOn = autosaveEnabled;
        if (autosaveIntervalSlider) autosaveIntervalSlider.value = autosaveIntervalID;
        if (saveFolderInput) saveFolderInput.text = saveFolder;

        ApplySaveAndLoad(autosaveEnabled, autosaveIntervalID, saveFolder);
    }

    public void OpenSettings()
    {
        currentPage = 1;
        RefreshSaves();
        gm.UIAnimator.SetTrigger("opensettings");
    }

    public void CloseSettings()
    {
        gm.UIAnimator.SetTrigger("closesettings");
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

    public void ChangeUIVolume(float value)
    {
        UIVolume = value;
        PlayerPrefs.SetFloat(UI_KEY, value);
        ApplyVolumes();
    }

    public void ApplyVolumes()
    {
        float multiplier = isMuted ? 0f : 1f;
        am.musicVolume = mainVolume * musicVolume * multiplier;
        am.ambientVolume = mainVolume * ambientVolume * multiplier;
        am.SFXVolume = mainVolume * SFXVolume * multiplier;
        am.UIVolume = mainVolume * UIVolume * multiplier;

        if (mainValue) mainValue.text = mainVolume.ToString("F1");
        if (musicValue) musicValue.text = musicVolume.ToString("F1");
        if (ambientValue) ambientValue.text = ambientVolume.ToString("F1");
        if (SFXValue) SFXValue.text = SFXVolume.ToString("F1");
        if (UIValue) UIValue.text = UIVolume.ToString("F1");

        am.ApplyVolumes();
    }

    public void ChangeMute(bool value)
    {
        isMuted = value;
        PlayerPrefs.SetInt(MUTE_KEY, value ? 1 : 0);
        ApplyVolumes();
    }

    public void ChangeAutosaveEnabled(bool value)
    {
        PlayerPrefs.SetInt(AUTOSAVE_ENABLED_KEY, value ? 1 : 0);
        string currentFolder = saveFolderInput ? saveFolderInput.text : (sal != null ? sal.saveFolder : "Saves");
        ApplySaveAndLoad(value, autosaveIntervalID, currentFolder);
    }

    public void ChangeAutosaveInterval(float value)
    {
        autosaveIntervalID = Mathf.RoundToInt(value);
        PlayerPrefs.SetInt(AUTOSAVE_INTERVAL_ID_KEY, autosaveIntervalID);
        bool enabled = autosaveToggle ? autosaveToggle.isOn : true;
        string currentFolder = saveFolderInput ? saveFolderInput.text : (sal != null ? sal.saveFolder : "Saves");
        ApplySaveAndLoad(enabled, autosaveIntervalID, currentFolder);
    }

    public void ChangeSaveFolder(string value)
    {
        PlayerPrefs.SetString(SAVE_FOLDER_KEY, value);
        bool enabled = autosaveToggle ? autosaveToggle.isOn : true;
        ApplySaveAndLoad(enabled, autosaveIntervalID, value);
    }

    void ApplySaveAndLoad(bool enabled, int intervalID, string folder)
    {
        if (sal == null) sal = FindObjectOfType<SaveAndLoad>();

        if (sal != null)
        {
            sal.autosaveEnabled = enabled;
            if (intervalID >= 0 && intervalID < autosaveIntervalValues.Length)
                sal.autosaveInterval = autosaveIntervalValues[intervalID];
            sal.saveFolder = folder;

            try
            {
                if (!string.IsNullOrEmpty(sal.SaveFolderPath))
                    System.IO.Directory.CreateDirectory(sal.SaveFolderPath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to create save directory: {ex.Message}");
            }
        }

        if (autosaveIntervalValue)
        {
            if (intervalID >= 0 && intervalID < autosaveIntervalValues.Length)
            {
                float seconds = autosaveIntervalValues[intervalID];
                autosaveIntervalValue.text = (seconds / 60f).ToString("F0");
            }
        }
    }

    public void RefreshSaves()
    {
        if (sal == null) sal = FindObjectOfType<SaveAndLoad>();
        if (sal == null) return;

        Dictionary<string, GardenSaveData> mySaves = sal.GetAllSaves();
        saveList = new List<KeyValuePair<string, GardenSaveData>>(mySaves);
        saveList.Sort((a, b) => b.Value.savedAt.CompareTo(a.Value.savedAt));

        totalPage = Mathf.Max(1, Mathf.CeilToInt(saveList.Count / 4f));
        currentPage = Mathf.Clamp(currentPage, 1, totalPage);

        UpdatePageDisplay();
    }

    public void NextPage()
    {
        if (currentPage < totalPage)
        {
            currentPage++;
            UpdatePageDisplay();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            UpdatePageDisplay();
        }
    }

    void UpdatePageDisplay()
    {
        if (holder == null) return;

        // Clear existing versions
        foreach (Transform child in holder)
        {
            Destroy(child.gameObject);
        }

        // Display current page saves
        int startIndex = (currentPage - 1) * 4;
        int endIndex = Mathf.Min(startIndex + 4, saveList.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            var save = saveList[i];
            if (saveVersion != null)
            {
                GameObject go = Instantiate(saveVersion, holder);
                SaveVersion sv = go.GetComponent<SaveVersion>();
                if (sv != null)
                {
                    sv.Init(save.Key, save.Value, sal);
                }
            }
        }

        // Update page indicator text
        if (pageIndicator != null)
        {
            pageIndicator.text = currentPage.ToString() + "/" + totalPage.ToString();
        }
    }
}