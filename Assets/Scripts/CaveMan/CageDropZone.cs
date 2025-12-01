using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

public class CageDropZone : MonoBehaviour
{
    [Header("Visual Settings")]
    public GameObject collectedMushroomsVisual;

    [Header("Cinematic Settings")]
    public PlayableDirector meteorCutscene;
    public Transform meteorTargetObject; // The object the camera will look at

    [Header("Cinematic Movement")]
    public float backwardDistance = 3.0f; // Distance to move backward
    public float backwardSpeed = 2.0f;    // Speed of backward movement

    [Header("Player Controls")]
    public MonoBehaviour movementScript;  // Reference to the Player Movement Script
    public MouseLook mouseLookScript;     // Reference to the MouseLook Script
    public Transform playerCamera;        // Reference to the Main Camera
    public Transform playerBody;          // Reference to the Player's main body (Root object)

    // --- OnGUI Variables for Black Screen ---
    private Texture2D blackTexture;
    private float fadeAlpha = 0f;  // 0 = Transparent, 1 = Black

    private bool isTriggered = false;
    private bool isLockedOnMeteor = false;

    void Start()
    {
        // Create a 1x1 black texture for the screen fade effect
        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();
    }

    void OnGUI()
    {
        // Draw the black texture over the whole screen if alpha > 0
        if (fadeAlpha > 0)
        {
            GUI.color = new Color(0, 0, 0, fadeAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
        }
    }

    private void Update()
    {
        // During the cinematic, rotate both CAMERA and BODY towards the meteor
        if (isLockedOnMeteor && playerCamera != null && meteorTargetObject != null)
        {
            Vector3 direction = meteorTargetObject.position - playerCamera.position;

            // 1. Rotate the Camera (Including Up/Down)
            Quaternion targetCamRotation = Quaternion.LookRotation(direction);
            playerCamera.rotation = Quaternion.Slerp(playerCamera.rotation, targetCamRotation, Time.deltaTime * 3f);

            // 2. Rotate the Body (Only Y-axis / Left-Right)
            if (playerBody != null)
            {
                Vector3 bodyDir = direction;
                bodyDir.y = 0; // Keep the body upright
                if (bodyDir != Vector3.zero)
                {
                    Quaternion targetBodyRotation = Quaternion.LookRotation(bodyDir);
                    playerBody.rotation = Quaternion.Slerp(playerBody.rotation, targetBodyRotation, Time.deltaTime * 3f);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            if (GameManager.instance.currentScore >= GameManager.instance.targetMushroomCount)
            {
                isTriggered = true;

                // Show visuals and trigger win state
                if (collectedMushroomsVisual != null) collectedMushroomsVisual.SetActive(true);
                GameManager.instance.WinGame();

                if (playerBody == null) playerBody = other.transform;

                ToggleControls(false);
                StartCoroutine(BlinkAndPlaySequence());
            }
            else
            {
                GameManager.instance.ShowWarning();
            }
        }
    }

    void ToggleControls(bool state)
    {
        if (movementScript != null)
        {
            movementScript.enabled = state;
            if (!state)
            {
                Rigidbody rb = movementScript.GetComponent<Rigidbody>();
                if (rb != null) rb.velocity = Vector3.zero;
            }
        }
        if (mouseLookScript != null) mouseLookScript.enabled = state;
    }

    IEnumerator MovePlayerBackward()
    {
        if (movementScript != null)
        {
            float traveled = 0f;
            CharacterController cc = movementScript.GetComponent<CharacterController>();

            while (traveled < backwardDistance)
            {
                float step = backwardSpeed * Time.deltaTime;
                Vector3 moveDir = -playerBody.forward * step;

                if (cc != null) cc.Move(moveDir);
                else playerBody.position += moveDir;

                traveled += step;
                yield return null;
            }
        }
    }

    IEnumerator BlinkAndPlaySequence()
    {
        // Settings for Blink Speeds
        float startBlinkSpeed = 0.5f; // Fast blink at start
        float endBlinkSpeed = 1.5f;   // Slow blink at end (Longer)

        // --- PHASE 1: START BLINK - CLOSE EYES ---
        float timer = 0f;
        while (timer < startBlinkSpeed)
        {
            timer += Time.deltaTime;
            fadeAlpha = Mathf.Lerp(0f, 1f, timer / startBlinkSpeed);
            yield return null;
        }
        fadeAlpha = 1f;

        yield return new WaitForSeconds(0.2f);

        isLockedOnMeteor = true;
        if (meteorCutscene != null) meteorCutscene.Play();

        // Start Moving Backward
        StartCoroutine(MovePlayerBackward());

        // --- PHASE 2: START BLINK - OPEN EYES ---
        timer = 0f;
        while (timer < startBlinkSpeed)
        {
            timer += Time.deltaTime;
            fadeAlpha = Mathf.Lerp(1f, 0f, timer / startBlinkSpeed);
            yield return null;
        }
        fadeAlpha = 0f;

        // --- PHASE 3: WATCH THE CUTSCENE ---
        if (meteorCutscene != null)
        {
            float remainingTime = (float)meteorCutscene.duration - startBlinkSpeed;
            if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);
        }

        Debug.Log("Cutscene finished. Starting End Blink sequence...");

        // --- PHASE 4: END BLINK - SLOW FADE OUT (CLOSE EYES) ---
        timer = 0f;
        while (timer < endBlinkSpeed)
        {
            timer += Time.deltaTime;
            fadeAlpha = Mathf.Lerp(0f, 1f, timer / endBlinkSpeed);
            yield return null;
        }
        fadeAlpha = 1f; // Screen is fully black

        // While screen is black, unlock camera and stop looking at meteor
        isLockedOnMeteor = false;

        // Wait a moment in darkness (Dazed effect)
        yield return new WaitForSeconds(1.0f);

        // --- PHASE 5: END BLINK - SLOW FADE IN (OPEN EYES) ---
        timer = 0f;
        while (timer < endBlinkSpeed)
        {
            timer += Time.deltaTime;
            fadeAlpha = Mathf.Lerp(1f, 0f, timer / endBlinkSpeed);
            yield return null;
        }
        fadeAlpha = 0f; // Screen is clear


        // --- PHASE 6: RETURN CONTROL ---
        ToggleControls(true);
        Debug.Log("Control returned to player.");

        GameManager.instance.ShowMeteorObjective();
    }
}