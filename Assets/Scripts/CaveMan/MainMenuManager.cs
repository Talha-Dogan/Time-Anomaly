using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject quitConfirmationPanel;
    public GameObject mainButtonsGroup;
    public GameObject settingsPanel; // --- NEW: Reference for the Settings/Options Panel

    [Header("Animation & Physics Settings")]
    public Animator characterAnimator;
    public Rigidbody characterRigidbody;
    public string animationTriggerName = "StartTrigger";

    [Header("Timing & Speed")]
    public float jumpDelay = 0.8f; // Time to wait for jump animation peak
    public float fallSpeed = 50f;  // Downward speed
    public float forwardSpeed = 20f; // Forward speed (How far he jumps)

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable physics initially
        if (characterRigidbody != null)
        {
            characterRigidbody.isKinematic = true;
            characterRigidbody.drag = 0;
        }

        if (MusicManager.instance != null) MusicManager.instance.PlayMenuMusic();
    }

    public void StartGame()
    {
        if (characterAnimator != null)
        {
            characterAnimator.SetTrigger(animationTriggerName);
        }

        StartCoroutine(JumpFallAndLoadRoutine());
    }

    IEnumerator JumpFallAndLoadRoutine()
    {
        // 1. STEP: Wait for the jump animation
        yield return new WaitForSeconds(jumpDelay);

        // 2. STEP: ENABLE PHYSICS AND LAUNCH
        if (characterRigidbody != null)
        {
            // Disable Animator so physics can take full control
            if (characterAnimator != null) characterAnimator.enabled = false;

            characterRigidbody.isKinematic = false; // Enable Gravity
            characterRigidbody.drag = 0f;

            // --- CALCULATE VELOCITY ---
            // Downward Vector (Falling)
            Vector3 downForce = Vector3.down * fallSpeed;

            // Forward Vector (Moving forward based on where character is looking)
            Vector3 forwardForce = characterRigidbody.transform.forward * forwardSpeed;

            // Combine both forces (Down + Forward)
            characterRigidbody.velocity = downForce + forwardForce;
        }

        if (MusicManager.instance != null) MusicManager.instance.PlayStartJingle();

        Debug.Log("ROCKET LAUNCH! Moving Forward & Down. Waiting for scene load...");

        // 3. STEP: Wait for impact and load scene
        yield return new WaitForSeconds(0.7f);

        if (MusicManager.instance != null) MusicManager.instance.PlayGameMusic();

        SceneManager.LoadScene("CaveMan");
    }

    // --- OPTIONS / SETTINGS LOGIC (NEW) ---
    public void OpenSettings()
    {
        // Hide the main buttons group
        if (mainButtonsGroup != null) mainButtonsGroup.SetActive(false);

        // Show the settings panel
        if (settingsPanel != null) settingsPanel.SetActive(true);

        Debug.Log("Settings Menu Opened!");
    }

    public void CloseSettings()
    {
        // Hide the settings panel
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Show the main buttons group again
        if (mainButtonsGroup != null) mainButtonsGroup.SetActive(true);

        Debug.Log("Settings Menu Closed!");
    }

    // --- QUIT LOGIC ---
    public void ShowQuitConfirmation()
    {
        quitConfirmationPanel.SetActive(true);
        if (mainButtonsGroup != null) mainButtonsGroup.SetActive(false);
        if (MusicManager.instance != null) MusicManager.instance.PlayQuitPanelMusic();
    }

    public void ConfirmQuit() { StartCoroutine(QuitRoutine()); }

    IEnumerator QuitRoutine()
    {
        quitConfirmationPanel.SetActive(false);
        if (MusicManager.instance != null) MusicManager.instance.PlaySadGoodbye();
        yield return new WaitForSeconds(5f);
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void CancelQuit()
    {
        quitConfirmationPanel.SetActive(false);
        if (mainButtonsGroup != null) mainButtonsGroup.SetActive(true);
        if (MusicManager.instance != null) MusicManager.instance.PlayCancelQuitMusic();
    }
}