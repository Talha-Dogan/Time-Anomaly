using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;

    // Ses seviyesi
    [Range(0f, 1f)]
    public float gruntVolume = 0.5f;

    public AudioClip[] cavemanSounds; // Homurdanma sesleri (Attack sesleri)

    [Header("Scream Settings (Dinozor Görünce)")]
    public AudioClip screamSound;
    public Transform dinosaur;
    public float detectionDistance = 15f;
    public float screamCooldown = 10f;
    private float fixedScreamVolume = 0.2f;

    [Header("Attack Settings (Saldırı Ayarı)")]
    // 3 vuruşta 1 gelmesi için bu değeri kodda kullanacağız.
    // %33 şans demek, istatistiksel olarak her 3 vuruşta 1 denk gelir.
    [Range(0, 100)]
    public int soundChancePercentage = 33;

    private float nextScreamTime = 0f;

    void Update()
    {
        CheckForDinosaur();

        // ARTIK YÜRÜME YOK, SADECE SALDIRI VAR
        // Sol Tık (0) genelde saldırıdır. Sen hangi tuşu kullanıyorsan değiştirebilirsin.
        if (Input.GetMouseButtonDown(0))
        {
            TryPlayAttackSound();
        }
    }

    void TryPlayAttackSound()
    {
        // Random.Range(0, 100) 0 ile 99 arasında bir sayı tutar.
        // Eğer bu sayı 33'ten küçükse (yani %33 ihtimalle) sesi çalar.
        if (Random.Range(0, 100) < soundChancePercentage)
        {
            PlayRandomVocal();
        }
    }

    void CheckForDinosaur()
    {
        if (dinosaur != null && screamSound != null)
        {
            float distance = Vector3.Distance(transform.position, dinosaur.position);

            if (distance < detectionDistance && Time.time >= nextScreamTime)
            {
                // Çığlık atarken diğer sesi bastırsın diye OneShot kullanabiliriz
                AudioSource.PlayClipAtPoint(screamSound, transform.position, fixedScreamVolume);
                nextScreamTime = Time.time + screamCooldown;
            }
        }
    }

    void PlayRandomVocal()
    {
        if (cavemanSounds != null && cavemanSounds.Length > 0 && audioSource != null)
        {
            // Eğer şu an zaten bir ses çalıyorsa onu kesmeyelim, üst üste binmesin.
            if (audioSource.isPlaying) return;

            // Rastgele bir homurdanma seç
            AudioClip randomClip = cavemanSounds[Random.Range(0, cavemanSounds.Length)];
            if (randomClip == null) return;

            audioSource.clip = randomClip;
            audioSource.volume = gruntVolume;

            // Hafif ton farkı (Her vuruşta aynı ses çıkmasın diye çok önemli)
            audioSource.pitch = Random.Range(0.85f, 1.15f);

            audioSource.Play();
        }
    }
}