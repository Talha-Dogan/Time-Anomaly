using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudioManager : MonoBehaviour
{
    [Header("References")]
    public ThirdPersonMovement playerMovement;

    [Header("Walk Audio Settings")]
    public AudioClip walkingLoopClip;

    [Range(0.1f, 3f)] public float walkPitch = 1.0f;
    [Range(0.1f, 3f)] public float sprintPitch = 1.5f;
    [Range(0.1f, 3f)] public float crouchPitch = 0.7f;

    [Header("Jump Audio Settings")]
    public AudioClip jumpSound;
    [Range(0.1f, 3f)] public float jumpMinPitch = 0.9f;
    [Range(0.1f, 3f)] public float jumpMaxPitch = 1.1f;

    private AudioSource footstepsSource;       // Yürüme sesi (loop)
    private AudioSource jumpSource;            // Zıplama sesi (tek seferlik)

    void Start()
    {
        // 1. AudioSource: Ayak sesleri için
        footstepsSource = GetComponent<AudioSource>();
        footstepsSource.clip = walkingLoopClip;
        footstepsSource.loop = true;
        footstepsSource.playOnAwake = false;

        // 2. AudioSource: Zıplama sesi için (Kodla oluşturuyoruz)
        jumpSource = gameObject.AddComponent<AudioSource>();
        jumpSource.playOnAwake = false;
        jumpSource.loop = false;

        // Player referansını bul
        if (playerMovement == null)
            playerMovement = GetComponent<ThirdPersonMovement>();
    }

    void Update()
    {
        if (playerMovement == null) return;

        HandleFootsteps();
    }

    void HandleFootsteps()
    {
        // KONTROL: Karakter havadaysa VEYA hareket etmiyorsa sesi durdur
        if (!playerMovement.isGrounded || !playerMovement.IsMoving())
        {
            StopFootsteps();
            return;
        }

        // Eğer ses çalmıyorsa başlat
        if (!footstepsSource.isPlaying)
        {
            footstepsSource.Play();
        }

        // Pitch (Hız) Ayarı
        if (playerMovement.isCrouching)
        {
            footstepsSource.pitch = crouchPitch;
        }
        else if (playerMovement.isSprinting)
        {
            footstepsSource.pitch = sprintPitch;
        }
        else
        {
            footstepsSource.pitch = walkPitch;
        }
    }

    public void PlayJumpSound()
    {
        StopFootsteps();  // Zıplarken yürüme sesini kes

        if (jumpSound != null)
        {
            // Rastgele bir tonlama ile zıplama sesi çal
            jumpSource.pitch = Random.Range(jumpMinPitch, jumpMaxPitch);
            jumpSource.PlayOneShot(jumpSound);
        }
    }

    void StopFootsteps()
    {
        if (footstepsSource.isPlaying)
        {
            footstepsSource.Stop();
        }
    }
}