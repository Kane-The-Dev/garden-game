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
    public bool transparentBackground = true;

    private RenderTexture renderTexture;

    IEnumerator Start()
    {
        if (captureCamera == null)
        {
            Debug.LogError("No capture camera assigned to IconGenerator.");
            yield break;
        }

        // Create output folder
        string folder = Path.Combine(Application.dataPath, outputFolder);

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        // Create RenderTexture
        renderTexture = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32);
        renderTexture.Create();

        CameraClearFlags originalClearFlags = captureCamera.clearFlags;
        Color originalBackgroundColor = captureCamera.backgroundColor;

        captureCamera.targetTexture = renderTexture;
        captureCamera.clearFlags = CameraClearFlags.SolidColor;

        // Always render with an opaque black background.
        // We'll remove it later.
        captureCamera.backgroundColor = Color.black;

        foreach (GameObject prefab in prefabs)
        {
            if (prefab == null)
                continue;

            Debug.Log("Preparing " + prefab.name);

            bool createdInstance = false;
            GameObject obj = prefab;

            // Instantiate if it's actually a prefab asset
            if (prefab.scene == default || !prefab.scene.IsValid())
            {
                obj = Instantiate(
                    prefab,
                    spawnPoint != null ? spawnPoint.position : Vector3.zero,
                    prefab.transform.rotation);

                createdInstance = true;
            }

            obj.SetActive(true);

            // Disable Growable scripts
            foreach (Growable growable in obj.GetComponentsInChildren<Growable>(true))
            {
                growable.enabled = false;
            }

            // Ensure Outline exists
            Outline outline = obj.GetComponent<Outline>();
            if (outline == null)
                outline = obj.AddComponent<Outline>();

            if (outline != null)
                outline.enabled = true;

            // Wait a few frames so Outline initializes
            yield return new WaitForSeconds(0.2f);
            yield return null;
            yield return null;
            yield return new WaitForEndOfFrame();

            Debug.Log("Capturing " + prefab.name);

            SaveCameraView(prefab.name);

            yield return new WaitForSeconds(0.2f);

            obj.SetActive(false);

            if (createdInstance)
                Destroy(obj);

            yield return new WaitForSeconds(0.2f);
        }

        captureCamera.clearFlags = originalClearFlags;
        captureCamera.backgroundColor = originalBackgroundColor;
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

        if (transparentBackground)
        {
            Color32[] pixels = image.GetPixels32();

            for (int i = 0; i < pixels.Length; i++)
            {
                // Make pure black pixels transparent.
                if (pixels[i].r == 0 &&
                    pixels[i].g == 0 &&
                    pixels[i].b == 0)
                {
                    pixels[i].a = 0;
                }
            }

            image.SetPixels32(pixels);
            image.Apply();
        }

        byte[] bytes = image.EncodeToPNG();

        string path = Path.Combine(
            Application.dataPath,
            outputFolder,
            fileName + ".png");

        File.WriteAllBytes(path, bytes);

        Destroy(image);

        RenderTexture.active = current;

        Debug.Log($"Saved icon: {path}");
    }
}