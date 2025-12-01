using UnityEngine;
using System.Collections;

public class MushroomAudio : MonoBehaviour
{
    [Header("Target AI")]
    public MushroomAI targetAI;

    [Header("Visual Settings (Light)")]
    public Light mushroomLight;
    public Color normalColor = new Color(1f, 0.5f, 0f); // Varsayılan Turuncu yaptım
    public Color hitColor = Color.blue;   // Bayılınca ve vurulunca Mavi
    public float hitFlashDuration = 0.2f;

    [Header("Mushroom Sounds")]
    public AudioClip roamSound;
    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioClip collectSound;

    [Header("Noise Settings")]
    public float minWaitTime = 2.0f;
    public float maxWaitTime = 8.0f;

    private AudioSource audioSource;
    private int previousHealth;
    private bool wasStunned = false;
    private bool wasCollecting = false;

    private Coroutine roamCoroutine;
    private Coroutine hitFlashCoroutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (targetAI == null)
            targetAI = GetComponent<MushroomAI>();

        if (mushroomLight == null)
            mushroomLight = GetComponentInChildren<Light>();

        // Başlangıç rengini ayarla (Turuncu)
        if (mushroomLight != null)
        {
            mushroomLight.color = normalColor;
        }

        if (targetAI != null)
        {
            previousHealth = targetAI.maxHealth;
            StartRoamCycle();
        }
    }

    private void Update()
    {
        if (targetAI == null) return;

        // 1. DURUM: HASAR ALDI MI?
        if (targetAI.currentHealth < previousHealth)
        {
            PlayHitSound();
            previousHealth = targetAI.currentHealth;
        }

        // 2. DURUM: BAYILDI MI? (STUN BAŞLADI)
        if (targetAI.isStunned && !wasStunned)
        {
            PlayDeathSound(); // Sesini çal
            wasStunned = true;
            StopRoamCycle(); // Mırıldanmayı kes

            // --- YENİ EKLENEN KISIM ---
            // Eğer o sırada vurulma efekti (flash) çalışıyorsa onu durdur
            if (hitFlashCoroutine != null) StopCoroutine(hitFlashCoroutine);

            // Işığı DİREKT MAVİYE sabitle (Sürekli yansın)
            if (mushroomLight != null) mushroomLight.color = hitColor;
        }
        // 3. DURUM: AYILDI MI? (STUN BİTTİ)
        else if (!targetAI.isStunned && wasStunned)
        {
            wasStunned = false;

            // --- YENİ EKLENEN KISIM ---
            // Ayılınca rengi tekrar TURUNCUYA (Normal) çevir
            if (mushroomLight != null) mushroomLight.color = normalColor;

            StartRoamCycle(); // Tekrar mırıldanmaya başla
        }

        // 4. DURUM: TOPLANIYOR MU?
        if (targetAI.isCollecting && !wasCollecting)
        {
            PlayCollectSound();
            wasCollecting = true;
            StopRoamCycle();

            // İstersen toplanınca ışığı kapatabilirsin
            if (mushroomLight != null) mushroomLight.enabled = false;
        }
    }

    private void StartRoamCycle()
    {
        if (roamCoroutine != null) return;
        roamCoroutine = StartCoroutine(RoamRoutine());
    }

    private void StopRoamCycle()
    {
        if (roamCoroutine != null)
        {
            StopCoroutine(roamCoroutine);
            roamCoroutine = null;
        }
    }

    IEnumerator RoamRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            if (!targetAI.isStunned && !targetAI.isCollecting && roamSound != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(roamSound);
                yield return new WaitForSeconds(roamSound.length);
            }
        }
    }

    private void PlayHitSound()
    {
        // Eğer zaten baygınsa tekrar vurulma efekti yapma (Rengi bozulmasın)
        if (targetAI.isStunned || targetAI.isCollecting) return;

        audioSource.pitch = 1f;
        audioSource.PlayOneShot(hitSound);

        // Yanıp sönme efektini başlat
        if (hitFlashCoroutine != null) StopCoroutine(hitFlashCoroutine);
        hitFlashCoroutine = StartCoroutine(HitFlashRoutine());
    }

    IEnumerator HitFlashRoutine()
    {
        if (mushroomLight == null) yield break;

        // 1. Mavi yap
        mushroomLight.color = hitColor;

        // 2. Bekle
        yield return new WaitForSeconds(hitFlashDuration);

        // 3. Eski rengine (Turuncu) dön
        // Sadece baygın DEĞİLSE geri dön. Eğer bu sırada bayıldıysa mavi kalsın.
        if (!targetAI.isStunned)
        {
            mushroomLight.color = normalColor;
        }

        hitFlashCoroutine = null;
    }

    private void PlayDeathSound()
    {
        StopRoamCycle();
        audioSource.Stop();
        audioSource.pitch = 0.8f;
        audioSource.loop = false;
        audioSource.clip = deathSound;
        audioSource.Play();
    }

    private void PlayCollectSound()
    {
        StopRoamCycle();
        audioSource.Stop();
        audioSource.pitch = 1.2f;
        audioSource.loop = false;
        if (collectSound != null) audioSource.PlayOneShot(collectSound);
    }
}