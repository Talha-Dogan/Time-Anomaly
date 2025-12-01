using UnityEngine;
using System.Collections;

public class SimpleFireExtinguisher : MonoBehaviour
{
    [Header("Assignable Objects")]
    public GameObject targetFire;
    public GameObject fireHydrant;
    public GameObject waterVFX;

    [Header("Rewards")]
    public GameObject crystalPrefab;

    [Header("Settings")]
    public float hitRange = 3.0f;
    public float extinguishDelay = 2.0f;
    public float attackDuration = 1.0f;
    public float impactDelay = 0.5f;

    [Header("Pop Up Settings")]
    public float popForce = 8f;
    public float popDelay = 0.5f;

    [Header("Audio Settings")] // --- YENİ SES AYARI ---
    public AudioClip waterSound; // Buraya su fışkırma sesini sürükle

    private Animator animator;
    private ThirdPersonMovement movementScript;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        movementScript = GetComponent<ThirdPersonMovement>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && movementScript != null && movementScript.canMove)
        {
            StartCoroutine(PerformAttackSequence());
        }
    }

    IEnumerator PerformAttackSequence()
    {
        if (movementScript != null) movementScript.canMove = false;
        if (animator != null) animator.SetTrigger("Attack");

        yield return new WaitForSeconds(impactDelay);

        CheckHitLogic();

        float remainingDuration = attackDuration - impactDelay;
        if (remainingDuration > 0)
        {
            yield return new WaitForSeconds(remainingDuration);
        }

        if (movementScript != null) movementScript.canMove = true;
    }

    void CheckHitLogic()
    {
        float distance = Vector3.Distance(transform.position, fireHydrant.transform.position);

        if (distance <= hitRange)
        {
            Debug.Log("Hit Successful! Water flows...");
            StartCoroutine(ExtinguishFireRoutine());
        }
    }

    IEnumerator ExtinguishFireRoutine()
    {
        // --- 1. SESİ ÇAL (YENİ) ---
        // Sesi doğrudan musluğun olduğu pozisyonda oluşturur
        if (waterSound != null)
        {
            AudioSource.PlayClipAtPoint(waterSound, fireHydrant.transform.position);
        }

        // 2. Su efektini çıkar
        if (waterVFX != null)
        {
            GameObject vfx = Instantiate(waterVFX, fireHydrant.transform.position + Vector3.up, Quaternion.identity);
            Destroy(vfx, 4f);
        }

        // 3. Yangının sönmesini bekle
        yield return new WaitForSeconds(extinguishDelay);

        // 4. Yangını kapat
        if (targetFire != null)
        {
            targetFire.SetActive(false);
        }

        // --- POP UP SEQUENCE BAŞLIYOR ---

        // 5. Önce Yangın Musluğunu Fırlat
        if (fireHydrant != null)
        {
            PopObjectUp(fireHydrant);
        }

        // 6. Biraz bekle
        yield return new WaitForSeconds(popDelay);

        // 7. Sonra Kristali Oluştur ve Fırlat
        if (crystalPrefab != null)
        {
            GameObject gem = Instantiate(crystalPrefab, fireHydrant.transform.position + Vector3.up, Quaternion.identity);
            PopObjectUp(gem);
        }
    }

    void PopObjectUp(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody>();
        }

        rb.AddForce(Vector3.up * popForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
    }

    void OnDrawGizmosSelected()
    {
        if (fireHydrant != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(fireHydrant.transform.position, hitRange);
        }
    }
}