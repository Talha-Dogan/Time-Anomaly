using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class SimpleCarSmash : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public AmbulanceManager manager;

    [Header("Settings")]
    public float hitRange = 4.0f;
    public float flySpeedX = -30f;
    public float flyLiftY = 5f;

    [Header("Timing Settings")]
    public float impactDelay = 1.0f;

    [Header("Audio Settings")]
    public AudioClip smashSound;

    [Header("VFX Settings")] // --- YENİ EKLENEN KISIM ---
    public GameObject smashEffectPrefab; // Buraya patlama/kıvılcım prefabını sürükle

    private Rigidbody rb;
    private AudioSource audioSource;
    private bool isSmashed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        rb.isKinematic = true;
    }

    void Update()
    {
        if (isSmashed) return;

        if (Input.GetButtonDown("Fire1"))
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            if (distance <= hitRange)
            {
                StartCoroutine(SmashSequence());
            }
        }
    }

    IEnumerator SmashSequence()
    {
        // Sopanın inmesini bekle
        yield return new WaitForSeconds(impactDelay);

        // ŞİMDİ PARÇALA!
        SmashTheCar();
    }

    void SmashTheCar()
    {
        isSmashed = true;
        Debug.Log("Impact Frame Reached! Car Flying!");

        // 1. SESİ ÇAL
        if (smashSound != null)
        {
            audioSource.PlayOneShot(smashSound);
        }

        // 2. VFX EFEKTİNİ PATLAT (YENİ)
        if (smashEffectPrefab != null)
        {
            // Efekti arabanın tam olduğu yerde oluştur
            GameObject vfx = Instantiate(smashEffectPrefab, transform.position, Quaternion.identity);

            // Efekti 2 saniye sonra sahneden sil (Boşuna yer kaplamasın)
            Destroy(vfx, 2.0f);
        }

        // 3. FİZİĞİ AÇ VE UÇUR
        rb.isKinematic = false;

        Vector3 forceDirection = new Vector3(flySpeedX, flyLiftY, 0);
        rb.velocity = Vector3.zero;
        rb.velocity = forceDirection;

        rb.AddTorque(transform.forward * 10f, ForceMode.Impulse);

        if (manager != null)
        {
            manager.CarCleared();
        }

        Destroy(gameObject, 3f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRange);
    }
}