using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GasStationEvent : MonoBehaviour
{
    [Header("Required Objects")]
    public GameObject npcCharacter;
    public GameObject crystalItem;
    public GameObject dialogueBox;
    public TMP_Text dialogueText;

    [Header("Audio Settings")]
    public AudioClip windowHitSound; // Drag your glass/thump sound here
    [Range(0f, 1f)] public float soundVolume = 1.0f;

    [Header("Settings")]
    public float impactDelay = 1.0f;        // Syncs with your attack animation hit moment
    public float delayBeforeSpeak = 0.5f;
    public float readingTime = 3.5f;

    private bool isPlayerInRange = false;
    private bool hasTriggered = false;

    private void Update()
    {
        // If player is in trigger zone + presses Fire + hasn't done it yet
        if (isPlayerInRange && !hasTriggered && Input.GetButtonDown("Fire1"))
        {
            hasTriggered = true;
            StartCoroutine(StartEventSequence());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    IEnumerator StartEventSequence()
    {
        Debug.Log("Event Started: Player swings weapon...");

        // Step 0: Wait for the weapon to visually hit the window
        yield return new WaitForSeconds(impactDelay);

        // --- PLAY SOUND HERE (Synced with impact) ---
        if (windowHitSound != null)
        {
            // PlayClipAtPoint creates a temporary object to play the sound at this location
            AudioSource.PlayClipAtPoint(windowHitSound, transform.position, soundVolume);
        }

        Debug.Log("Impact! Sound played.");

        // Step 1: Reveal the NPC
        if (npcCharacter != null)
            npcCharacter.SetActive(true);

        // Small delay before text (NPC looks at player)
        yield return new WaitForSeconds(delayBeforeSpeak);

        // Step 2: First Dialogue Line
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
            if (dialogueText != null)
                dialogueText.text = "OI! Are you a caveman?! Stop banging on the glass!";
        }

        yield return new WaitForSeconds(readingTime);

        // Step 3: Second Dialogue Line + Give Crystal
        if (crystalItem != null)
            crystalItem.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = "Here, take this crystal and go back to your cave!";

        yield return new WaitForSeconds(readingTime);

        // Step 4: NPC Disappears
        if (npcCharacter != null)
            npcCharacter.SetActive(false);

        // Step 5: Final Text
        if (dialogueText != null)
            dialogueText.text = "Take it and go now.";

        yield return new WaitForSeconds(2.0f);

        // Step 6: Close Dialogue
        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        Debug.Log("Event Finished.");
    }
}