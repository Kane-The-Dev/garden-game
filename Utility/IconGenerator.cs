using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class IconGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public List<GameObject> prefabs;

    [Header("Scene References")]
    public Transform spawnPoint;
    public Camera captureCamera;

    [Header("Output")]
    public int resolution = 256;
    public string outputFolder = "Resources/Icons", postfix = "";
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

        // Pre-render with an opaque black background and remove it later.
        captureCamera.backgroundColor = Color.black;

        foreach (GameObject obj in prefabs)
        {
            if (obj == null)
                continue;

            Debug.Log("Preparing " + obj.name);

            obj.SetActive(true);

            // Wait a few frames so Outline initializes
            yield return new WaitForSeconds(0.2f);

            Debug.Log("Capturing " + obj.name);

            SaveCameraView(obj.name);

            yield return new WaitForSeconds(0.2f);

            obj.SetActive(false);

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
            fileName + postfix + ".png");

        File.WriteAllBytes(path, bytes);

        Destroy(image);

        RenderTexture.active = current;

        Debug.Log($"Saved icon: {path}");
    }

    // Toggle Read/Write on off to prepare the model for capture
#if UNITY_EDITOR
    [ContextMenu("Toggle Readable On")]
    public void ToggleReadableOn()
    {
        SetModelsReadable(true);
    }

    [ContextMenu("Toggle Readable Off")]
    public void ToggleReadableOff()
    {
        SetModelsReadable(false);
    }

    void SetModelsReadable(bool readable)
    {
        HashSet<string> paths = new HashSet<string>();

        foreach (GameObject obj in prefabs)
        {
            if (obj == null)
                continue;

            foreach (MeshFilter mf in obj.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh != null)
                {
                    string p = AssetDatabase.GetAssetPath(mf.sharedMesh);
                    if (!string.IsNullOrEmpty(p))
                        paths.Add(p);
                }
            }

            foreach (SkinnedMeshRenderer smr in obj.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (smr.sharedMesh != null)
                {
                    string p = AssetDatabase.GetAssetPath(smr.sharedMesh);
                    if (!string.IsNullOrEmpty(p))
                        paths.Add(p);
                }
            }
        }

        foreach (string path in paths)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
                continue;

            importer.isReadable = readable;
            importer.SaveAndReimport();

            Debug.Log($"Set Read/Write = {readable} on model: {path}");
        }

        AssetDatabase.Refresh();
    }
#endif
}