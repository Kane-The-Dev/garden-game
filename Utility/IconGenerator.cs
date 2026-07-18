using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class IconGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public List<GameObject> prefabs;

    [Header("Scene References")]
    public Transform spawnPoint;
    public Camera captureCamera;

    [Header("Output")]
    public int resolution = 256;
    public string outputFolder = "Resources/Icons";

    private RenderTexture renderTexture;

    IEnumerator Start()
    {
        // Create output folder
        string folder = Path.Combine(Application.dataPath, outputFolder);

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        // Create RenderTexture
        renderTexture = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32);
        captureCamera.targetTexture = renderTexture;

        foreach (GameObject prefab in prefabs)
        {
            if (prefab == null)
                continue;

            Debug.Log("Preparing " + prefab.name);

            GameObject obj = prefab;
            obj.SetActive(true);

            // Disable growable behavior so it stays static during capture
            foreach (Growable growable in obj.GetComponentsInChildren<Growable>(true))
            {
                growable.enabled = false;
            }

            // Add outline if the QuickOutline component exists
            Outline outline = obj.GetComponent<Outline>();
            if (outline == null)
                outline = obj.AddComponent<Outline>();

            if (outline == null)
                Debug.LogWarning($"Could not add Outline component to {prefab.name}");

            // Wait for Awake/Start and a couple of frames so the outline materials are ready
            yield return new WaitForSeconds(0.5f);
            yield return null;
            yield return null;
            yield return new WaitForEndOfFrame();

            Debug.Log("Capturing " + prefab.name);
            SaveCameraView(prefab.name);

            // Wait after saving
            yield return new WaitForSeconds(0.5f);

            Debug.Log("Disabling " + prefab.name);
            obj.SetActive(false);

            // Wait before next prefab
            yield return new WaitForSeconds(0.5f);
        }

        captureCamera.targetTexture = null;
        renderTexture.Release();

        Debug.Log("Finished!");
    }

    void SaveCameraView(string fileName)
    {
        RenderTexture current = RenderTexture.active;

        RenderTexture.active = renderTexture;

        captureCamera.Render();

        Texture2D image = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            false);

        image.ReadPixels(
            new Rect(0, 0, resolution, resolution),
            0,
            0);

        image.Apply();

        byte[] bytes = image.EncodeToPNG();

        string path = Path.Combine(
            Application.dataPath,
            outputFolder,
            fileName + ".png");

        File.WriteAllBytes(path, bytes);

        Destroy(image);

        RenderTexture.active = current;

        Debug.Log($"Saving to: {path}");
        Debug.Log($"File exists after save: {File.Exists(path)}");
    }
}
