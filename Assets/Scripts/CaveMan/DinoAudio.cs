using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DinoAudio : MonoBehaviour
{
    [Header("Dino Sounds")]
    public AudioClip roarSound;   // Sound when spotting the player
    public AudioClip biteSound;   // Sound when killing the player

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // --- PUBLIC METHODS ---

    public void PlayRoar()
    {
        // Avoid spamming the roar if it's already playing
        if (audioSource.isPlaying && audioSource.clip == roarSound) return;

        audioSource.Stop();
        // Randomize pitch slightly for a more natural, scary effect
        audioSource.pitch = Random.Range(0.8f, 1.0f);
        audioSource.PlayOneShot(roarSound);
    }

    public void PlayBite()
    {
        audioSource.Stop();
        // Bite sound can be faster/sharper
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(biteSound);
    }
}