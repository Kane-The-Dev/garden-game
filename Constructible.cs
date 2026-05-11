using UnityEngine;

public class Constructible : MonoBehaviour
{
    [Header("Removal")]
    public ChopTool myChopTool;
    public bool chopped;
    public float chopIndex;
    int chopStage = 0;
    public float STAGE1 = 0.3333f;
    public float STAGE2 = 0.6666f;
    public float STAGE3 = 1f;

    [Header("Visual Effects")]
    [SerializeField] GameObject debris;
    [SerializeField] Transform effectSpawnPoint;
    public float shakeAmplitude;
    Vector2 shakeDirection;

    [Header("Sound Effects")]
    [SerializeField] AdvancedAudioSource myAAS;
    [SerializeField] AudioClip[] construct, impact, demolish;

    [Header("Other")]
    public bool isPreview = true;
    GameManager gm;

    void Start()
    {
        gm = GameManager.instance;
        if (myAAS && construct.Length > 0) myAAS.PlayOneShot(construct[0], 1f, true);
    }

    void Update()
    {
        Chopping();
        Shaking();
    }

    void Chopping()
    {
        if (chopped) return;

        if (chopIndex > 0f)
            chopIndex -= Time.deltaTime / 3f;

        chopIndex = Mathf.Clamp01(chopIndex);

        // Determine stage from chopIndex
        int newStage;
        if (chopIndex >= STAGE3)
            newStage = 3;
        else if (chopIndex >= STAGE2)
            newStage = 2;
        else if (chopIndex >= STAGE1)
            newStage = 1;
        else
            newStage = 0;

        if (newStage > chopStage)
        {
            gm.cam.ScreenShake(0.02f);
            myChopTool.PlayChop();

            // Stage 0 → 1
            if (chopStage < 1 && newStage >= 1) Shake(3f, 1);

            // Stage 1 → 2
            if (chopStage < 2 && newStage >= 2) Shake(3f, 1);

            // Stage 2 → 3 (final)
            if (newStage == 3) Chop();
        }

        chopStage = newStage;
    }

    void Chop()
    {
        if (chopped) return;
        chopped = true;
        Shake(3f, 3);

        GameObject model = transform.GetChild(0).gameObject;
        GameObject root = transform.parent?.gameObject;
        Destroy(model);
        if (root != null) Destroy(root, 5f);

        if (myAAS && demolish.Length > 0) {
            int i = Random.Range(0, demolish.Length);
            myAAS.PlayOneShot(demolish[i], 0.3f, true);
        }
    }

    void Shaking()
    {
        if (shakeAmplitude >= 0f)
        {
            shakeAmplitude -= 10f * Time.deltaTime;
            float rotX = Mathf.Sin(Time.time * 10f) * shakeAmplitude * shakeDirection.x;
            float rotZ = Mathf.Sin(Time.time * 10f) * shakeAmplitude * shakeDirection.y;
            transform.rotation = Quaternion.Euler(
                rotX, 
                transform.eulerAngles.y, 
                rotZ
            );
        }
    }

    public void Shake(float amplitude, int debrisCount)
    {
        if (myAAS && impact.Length > 0) 
            myAAS.PlayOneShot(impact[Random.Range(0, impact.Length)], 1f, true);

        for (int i = 0; i < debrisCount; i++)
            if (debris && effectSpawnPoint)
                Instantiate(
                    debris,
                    effectSpawnPoint.position,
                    Quaternion.identity
                );
        
        shakeAmplitude = amplitude;
        shakeDirection = Random.insideUnitCircle.normalized;
    }
}
