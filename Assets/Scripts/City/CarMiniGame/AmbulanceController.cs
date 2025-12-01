using UnityEngine;
using System.Collections;

public class AmbulanceController : MonoBehaviour
{
    [Header("Lights")]
    public Light redLight;
    public Light blueLight;
    public float flashSpeed = 0.5f;

    [Header("Movement")]
    public float driveSpeed = 15f;
    private bool isDriving = false;

    [Header("Audio Settings")] // --- NEW SECTION ---
    public AudioSource sirenAudioSource; // Drag AudioSource component here
    public AudioClip sirenClip;          // Drag your .mp3/.wav sound here

    // Internal variables
    private float timer;
    private bool isRedActive = true;

    void Start()
    {
        // Light Setup
        if (redLight) redLight.enabled = true;
        if (blueLight) blueLight.enabled = false;

        // --- NEW: AUDIO SETUP ---
        if (sirenAudioSource != null && sirenClip != null)
        {
            sirenAudioSource.clip = sirenClip;
            sirenAudioSource.loop = true; // Ensure the siren repeats
            sirenAudioSource.Play();      // Start playing immediately
        }
    }

    void Update()
    {
        HandleSiren();

        if (isDriving)
        {
            // Move Forward relative to rotation
            transform.Translate(Vector3.forward * driveSpeed * Time.deltaTime);
        }
    }

    void HandleSiren()
    {
        timer += Time.deltaTime;

        if (timer >= flashSpeed)
        {
            isRedActive = !isRedActive;
            if (redLight) redLight.enabled = isRedActive;
            if (blueLight) blueLight.enabled = !isRedActive;
            timer = 0f;
        }
    }

    public void DriveAway()
    {
        StartCoroutine(DepartureRoutine());
    }

    IEnumerator DepartureRoutine()
    {
        Debug.Log("Path Cleared! Waiting 5 seconds...");
        yield return new WaitForSeconds(5.0f);

        Debug.Log("GO GO GO!");
        flashSpeed = 0.05f; // Fast flash

        // --- OPTIONAL: Increase Pitch for urgency ---
        if (sirenAudioSource != null) sirenAudioSource.pitch = 1.1f;

        isDriving = true;

        // Destroy after 15 seconds
        Destroy(gameObject, 15f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TurnRight"))
        {
            Debug.Log("Turning Right!");
            transform.Rotate(0, 90, 0);
        }
    }
}