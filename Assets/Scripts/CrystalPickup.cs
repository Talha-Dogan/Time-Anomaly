using UnityEngine;
using System.Collections;

public class CrystalPickup : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Which mission ID from the MissionManager list will this crystal complete?")]
    public int missionID;

    [Header("Audio Settings")]
    public AudioClip pickupSound; // Drag your sound file here in the Inspector

    [Header("Animation Settings")]
    public float pickupDelay = 0.5f;     // Time to wait before item disappears (synced with hand grab)
    public float totalAnimTime = 2.0f;   // Total time to lock player movement

    private bool isPlayerInRange = false;
    private Animator playerAnimator;           // Reference to Player's Animator
    private ThirdPersonMovement playerMovement; // Reference to Player's movement script

    private void Update()
    {
        // If player is in range and presses 'E'
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // Start the timed sequence
            StartCoroutine(CollectSequence());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            // Automatically find player scripts
            playerAnimator = other.GetComponentInChildren<Animator>();
            playerMovement = other.GetComponent<ThirdPersonMovement>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // Clear references when leaving area (Optional)
            playerAnimator = null;
            playerMovement = null;
        }
    }

    // --- SEQUENTIAL PICKUP LOGIC ---
    IEnumerator CollectSequence()
    {
        // 1. Lock Player Movement (If script is found)
        if (playerMovement != null) playerMovement.canMove = false;

        // 2. Play Animation (Ensure your Animator has a Trigger named "PickUp")
        if (playerAnimator != null) playerAnimator.SetTrigger("PickUp");

        // 3. Wait until the hand visually reaches the object
        yield return new WaitForSeconds(pickupDelay);

        // 4. PLAY SOUND EFFECT
        // We use PlayClipAtPoint so the sound plays even if we disable the object immediately after
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // 5. Complete Mission
        if (MissionManager.instance != null)
        {
            MissionManager.instance.CompleteMission(missionID);
        }

        // 6. Make Crystal Invisible (Do not destroy yet, code needs to finish running)
        // Disable MeshRenderer to hide visuals
        if (GetComponent<MeshRenderer>() != null)
            GetComponent<MeshRenderer>().enabled = false;

        // Disable Collider so player can't interact again
        if (GetComponent<Collider>() != null)
            GetComponent<Collider>().enabled = false;

        // (If you have particle effects, you should disable them here too)

        // 7. Wait for the rest of the animation (Standing up time)
        yield return new WaitForSeconds(totalAnimTime - pickupDelay);

        // 8. Unlock Player Movement
        if (playerMovement != null) playerMovement.canMove = true;

        // 9. Destroy the object completely
        Destroy(gameObject);
    }
}   