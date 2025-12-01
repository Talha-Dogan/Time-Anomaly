using UnityEngine;
using System.Collections;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 4f;
    public LayerMask interactionLayer;

    [Header("Weapon References")]
    public GameObject hiddenClub;
    public GameObject hangingApple;

    [Header("Rewards")]
    public GameObject diamondPrefab;

    [Header("Audio Settings")]
    public AudioClip treeBreakSound; // Drag your sound file here!

    [Header("Fire Hydrant Settings")]
    public GameObject hitEffectPrefab;
    public GameObject targetFire;
    public float extinguishDelay = 1.0f;
    public float hydrantPopForce = 8f;

    [Header("Car Smash Settings")]
    public float carHitForce = 25f;
    private AmbulanceManager ambulanceManager;

    [Header("Attack Settings")]
    public float attackDuration = 1.5f;

    // Internal references
    private Animator animator;
    private bool hasWeapon = false;
    private ThirdPersonMovement movementScript;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        movementScript = GetComponent<ThirdPersonMovement>();
        ambulanceManager = FindObjectOfType<AmbulanceManager>();

        if (hiddenClub != null) hiddenClub.SetActive(false);
    }

    void Update()
    {
        // 1. INTERACT (Pick up weapon)
        if (Input.GetKeyDown(KeyCode.E) && !hasWeapon)
        {
            TryInteract();
        }

        // 2. ATTACK
        if (Input.GetButtonDown("Fire1") && hasWeapon)
        {
            if (movementScript == null || movementScript.canMove)
            {
                StartCoroutine(PerformAttackSequence());
            }
        }
    }

    IEnumerator PerformAttackSequence()
    {
        if (movementScript != null) movementScript.canMove = false;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(0.4f); // Hit moment
        CheckHitLogic();

        yield return new WaitForSeconds(attackDuration - 0.4f);

        if (movementScript != null) movementScript.canMove = true;
    }

    void CheckHitLogic()
    {
        // Ray origin düzeltildi (Yukarıdaki 1. adımı yapmayı unutma)
        Vector3 rayOrigin = transform.position + Vector3.up + (transform.forward * 0.5f);
        Ray ray = new Ray(rayOrigin, transform.forward);
        RaycastHit hit;

        // Debug Çizgisi: Kırmızı çizgiyi sahnede gör
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red, 2f);

        if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
        {
            // --- BURASI ÇOK ÖNEMLİ: Neye çarptığını konsola yazsın ---
            Debug.Log("Vurulan Şey: " + hit.collider.name + " | Tag: " + hit.collider.tag);
            // --------------------------------------------------------

            if (hit.collider.CompareTag("Hydrant"))
            {
                // Kodların devamı...
                // Hit VFX
                if (hitEffectPrefab != null)
                {
                    GameObject vfx = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(vfx, 2f);
                }

                StartCoroutine(ExtinguishFireRoutine(hit.collider.gameObject));
            }

            // --- 2. OBSTACLE CAR ---
            else if (hit.collider.CompareTag("ObstacleCar"))
            {
                Rigidbody carRb = hit.collider.GetComponent<Rigidbody>();
                if (carRb != null && carRb.isKinematic)
                {
                    carRb.isKinematic = false;
                    Vector3 flyDir = (hit.transform.position - transform.position).normalized + Vector3.up * 0.5f;
                    carRb.AddForce(flyDir * carHitForce, ForceMode.Impulse);
                    carRb.AddTorque(Random.insideUnitSphere * carHitForce, ForceMode.Impulse);

                    if (ambulanceManager != null) ambulanceManager.CarCleared();

                    Destroy(hit.collider.gameObject, 4f);
                }
            }
        }
    }

    IEnumerator ExtinguishFireRoutine(GameObject hydrantObj)
    {
        yield return new WaitForSeconds(extinguishDelay);

        if (targetFire != null) targetFire.SetActive(false);

        // 1. Pop Hydrant Up
        if (hydrantObj != null)
        {
            Rigidbody rb = hydrantObj.GetComponent<Rigidbody>();
            if (rb == null) rb = hydrantObj.AddComponent<Rigidbody>();

            rb.AddForce(Vector3.up * hydrantPopForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(0.5f);

        // 2. Spawn Diamond
        if (diamondPrefab != null && hydrantObj != null)
        {
            GameObject gem = Instantiate(diamondPrefab, hydrantObj.transform.position + Vector3.up, Quaternion.identity);

            Rigidbody gemRb = gem.GetComponent<Rigidbody>();
            if (gemRb != null)
            {
                gemRb.AddForce(Vector3.up * 6f, ForceMode.Impulse);
            }
        }
    }

    void TryInteract()
    {
        Ray ray = new Ray(transform.position + Vector3.up, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
        {
            if (hit.collider.CompareTag("Tree"))
            {
                if (animator != null) animator.SetTrigger("Interact");
                StartCoroutine(SwapWeaponSequence(hit.collider.gameObject));
                StartCoroutine(FreezeMovementFixed(1.5f));
            }
        }
    }

    IEnumerator SwapWeaponSequence(GameObject worldTree)
    {
        Vector3 treePos = worldTree.transform.position;
        yield return new WaitForSeconds(0.8f);

        // --- PLAY SOUND EFFECT HERE ---
        // We use PlayClipAtPoint because the object (the tree) is about to be destroyed.
        // If we used a standard AudioSource, the sound would cut off instantly.
        if (treeBreakSound != null)
        {
            AudioSource.PlayClipAtPoint(treeBreakSound, treePos);
        }

        Destroy(worldTree);

        // Spawn Crystal from tree
        if (diamondPrefab != null)
        {
            GameObject gem = Instantiate(diamondPrefab, treePos + Vector3.up * 0.5f, Quaternion.identity);
            Rigidbody gemRb = gem.GetComponent<Rigidbody>();
            if (gemRb != null) gemRb.AddForce(Vector3.up * 4f, ForceMode.Impulse);
        }

        if (hiddenClub != null) hiddenClub.SetActive(true);
        hasWeapon = true;
        if (animator != null) animator.SetBool("HasWeapon", true);
        if (hangingApple != null) hangingApple.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    IEnumerator FreezeMovementFixed(float duration)
    {
        if (movementScript != null) movementScript.canMove = false;
        yield return new WaitForSeconds(duration);
        if (movementScript != null) movementScript.canMove = true;
    }
}