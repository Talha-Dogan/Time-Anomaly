using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    #region Variables

    [Header("Movement Settings")]
    public float movementSpeed = 5.0f;
    public float gravity = -9.81f;

    [Header("Combat Settings")]
    public float attackCooldown = 0.7f;

    [Tooltip("How far the player can hit (in meters).")]
    public float attackRange = 2.5f;

    [Tooltip("Layers that can be hit (e.g., Default, Ground, Enemy).")]
    public LayerMask hitLayers;

    [Header("Interaction Settings")]
    [Tooltip("How close you need to be to collect items.")]
    public float interactionRange = 3.0f;

    [Tooltip("Layers that can be interacted with (Default is usually fine).")]
    public LayerMask interactionLayers;

    [Header("VFX Settings")]
    [Tooltip("The impact effect prefab.")]
    public GameObject hitVfxPrefab;

    [Tooltip("The TrailRenderer component on the stick.")]
    public TrailRenderer stickTrail;

    [Header("Weapon Setup")]
    public GameObject stickPrefab;
    public Transform stickParent;

    [Header("Components")]
    public Animator playerAnimator;

    // --- BURASI YENİ EKLENDİ (Kamera Sabitleme Ayarı) ---
    [Header("Camera Fix Settings")]
    [Tooltip("Buraya sabitlemek istediğin kamerayı veya CameraHolder'ı sürükle.")]
    public Transform cameraObjectToFix;

    [Tooltip("Kameranın durması gereken yer (X, Y, Z). Genelde kafa için 0, 1, 0 iyidir.")]
    public Vector3 fixedLocalPosition = new Vector3(0, 1, 0);
    // ----------------------------------------------------

    // Private variables
    private CharacterController controller;
    private Vector3 velocity;
    private bool canAttack = true;
    private Camera mainCam;

    #endregion

    #region Unity Methods

    void Start()
    {
        controller = GetComponent(typeof(CharacterController)) as CharacterController;
        mainCam = Camera.main;

        if (playerAnimator == null)
            playerAnimator = GetComponent<Animator>();

        SpawnStick();
        SetupTrail();
    }

    void Update()
    {
        // --- NEW: STOP INPUT IF PAUSED ---
        if (PauseMenu.GameIsPaused) return;
        // ---------------------------------

        HandleMovement();
        HandleCombat();
        HandleInteraction();
    }

    // --- YENİ EKLENDİ: KAMERAYI ÇİVİLEME ---
    // LateUpdate, hareket bittikten sonra çalışır, bu yüzden titreme yapmaz.
    void LateUpdate()
    {
        if (cameraObjectToFix != null)
        {
            // Kameranın yerel pozisyonunu (LocalPosition) zorla istediğimiz yere getiriyoruz.
            // Böylece Cinemachine veya başka bir şey onu kaydırırsa, bu kod geri düzeltir.
            if (cameraObjectToFix.localPosition != fixedLocalPosition)
            {
                cameraObjectToFix.localPosition = fixedLocalPosition;
            }
        }
    }
    // ---------------------------------------

    #endregion

    #region Custom Methods

    private void HandleMovement()
    {
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Yön vektörünü oluşturuyoruz
        Vector3 move = transform.right * x + transform.forward * z;

        // Çapraz gidince hızlanmayı engelle
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }

        controller.Move(move * movementSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleCombat()
    {
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            PerformAttack();
        }
    }

    private void HandleInteraction()
    {
        // When 'E' is pressed
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Shoot ray from center of screen
            Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            // Check if we hit something within interaction range
            if (Physics.Raycast(ray, out hit, interactionRange, interactionLayers))
            {
                // Try to find the MushroomAI script on the object or its parent
                MushroomAI mushroom = hit.collider.GetComponentInParent<MushroomAI>();

                if (mushroom != null)
                {
                    // Try to collect (Script will check if it's stunned)
                    mushroom.Collect();
                }
            }
        }
    }

    private void PerformAttack()
    {
        // 1. Visuals
        if (stickTrail != null) stickTrail.emitting = true;
        if (playerAnimator != null) playerAnimator.SetTrigger("Attack");

        // 2. Raycast Logic
        CheckForHit();

        // 3. Cooldown
        canAttack = false;
        Invoke(nameof(ResetAttackCooldown), attackCooldown);
    }

    private void CheckForHit()
    {
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, attackRange, hitLayers))
        {
            // A. Spawn VFX
            if (hitVfxPrefab != null)
            {
                GameObject vfx = Instantiate(hitVfxPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(vfx, 2.0f);
            }

            // B. Check for Mushroom
            MushroomAI mushroom = hit.collider.GetComponent<MushroomAI>();
            if (mushroom != null)
            {
                // Deal damage (1 point)
                mushroom.TakeDamage(1);
            }
        }
    }

    private void ResetAttackCooldown()
    {
        if (stickTrail != null) stickTrail.emitting = false;
        canAttack = true;
    }

    private void SpawnStick()
    {
        if (stickParent.childCount == 0 && stickPrefab != null)
            Instantiate(stickPrefab, stickParent);
    }

    private void SetupTrail()
    {
        if (stickParent != null)
            stickTrail = stickParent.GetComponentInChildren<TrailRenderer>();

        if (stickTrail != null)
            stickTrail.emitting = false;
    }

    #endregion
}