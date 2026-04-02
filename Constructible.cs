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
    float shakeAmplitude;
    Vector2 shakeDirection;

    [Header("Sound Effects")]
    AdvancedAudioSource myAAS;
    AudioClip[] impacts, breaks;
    GameManager gm;

    void Start()
    {
        gm = GameManager.instance;
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
            if (chopStage < 1 && newStage >= 1)
            {
                Shake(3f);
            }

            // Stage 1 → 2
            if (chopStage < 2 && newStage >= 2)
            {
                Shake(3f);
            }

            // Stage 2 → 3 (final)
            if (newStage == 3)
            {
                Chop();
                Destroy(gameObject, 5f);

                // myAAS.PlayOneShot(breaks[0], 1f, true);
            }
        }

        chopStage = newStage;
    }

    void Chop()
    {
        if (chopped) return;
        chopped = true;
        Debug.Log("Dead");
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

    public void Shake(float amplitude)
    {
        // myAAS.PlayOneShot(impacts[Random.Range(0, impacts.Length)], 1f, true);

        GameObject burst = Instantiate(debris, 
            effectSpawnPoint.position, 
            Quaternion.identity
        );
        
        shakeAmplitude = amplitude;
        shakeDirection = Random.insideUnitCircle.normalized;
    }
}
