using UnityEngine;
using System.Collections;

public class ATMController : MonoBehaviour
{
    [Header("Assignable Objects")]
    public GameObject targetATM;    // Sahnendeki ATM objesini buraya sürükle
    public GameObject moneyPrefab;  // Para prefabını buraya sürükle
    public Transform spawnPoint;    // Paranın çıkacağı nokta (ATM'nin çocuğu olan boş bir obje)

    [Header("Settings")]
    public float hitRange = 3.0f;       // Vuruş mesafesi
    public float attackDuration = 1.0f; // Animasyonun toplam süresi (Karakterin kilitli kalacağı süre)
    public float impactDelay = 0.4f;    // Vuruşun gerçekleşeceği an (Saniye cinsinden)
    public int moneyCount = 5;          // Her vuruşta kaç para çıksın

    private Animator animator;
    private ThirdPersonMovement movementScript;

    void Start()
    {
        // Player üzerindeki bileşenleri otomatik al
        animator = GetComponentInChildren<Animator>();
        movementScript = GetComponent<ThirdPersonMovement>();
    }

    void Update()
    {
        // Sol Tık Algılandığı An (Ve zaten kilitli değilsek)
        if (Input.GetButtonDown("Fire1") && movementScript != null && movementScript.canMove)
        {
            StartCoroutine(PerformAttackSequence());
        }
    }

    // --- SALDIRI DÖNGÜSÜ ---
    IEnumerator PerformAttackSequence()
    {
        // A) HAREKETİ KİLİTLE
        if (movementScript != null) movementScript.canMove = false;

        // B) ANİMASYONU OYNAT
        if (animator != null) animator.SetTrigger("Attack");

        // C) VURUŞ ANINA KADAR BEKLE
        yield return new WaitForSeconds(impactDelay);

        // D) MESAFE KONTROLÜ VE PARA ÇIKARMA
        CheckHitLogic();

        // E) ANİMASYONUN KALAN SÜRESİNİ BEKLE
        float remainingDuration = attackDuration - impactDelay;
        if (remainingDuration > 0)
        {
            yield return new WaitForSeconds(remainingDuration);
        }

        // F) HAREKETİ GERİ AÇ
        if (movementScript != null) movementScript.canMove = true;
    }

    void CheckHitLogic()
    {
        if (targetATM == null) return;

        // ATM ile karakter arasındaki mesafeyi ölç
        float distance = Vector3.Distance(transform.position, targetATM.transform.position);

        if (distance <= hitRange)
        {
            Debug.Log("Vuruş Başarılı! Paralar geliyor...");
            StartCoroutine(DispenseMoneyRoutine());
        }
        else
        {
            Debug.Log("Boşa Salladın! (ATM çok uzakta)");
        }
    }

    IEnumerator DispenseMoneyRoutine()
    {
        // Belirlenen sayıda para üret
        for (int i = 0; i < moneyCount; i++)
        {
            if (moneyPrefab != null)
            {
                // Paranın çıkacağı pozisyonu belirle
                Vector3 finalSpawnPos = (spawnPoint != null) ? spawnPoint.position : targetATM.transform.position + Vector3.up * 1.5f + targetATM.transform.forward * 0.5f;

                // Parayı oluştur
                GameObject money = Instantiate(moneyPrefab, finalSpawnPos, Random.rotation);

                // Parayı fırlat (Rigidbody kuvveti uygula)
                Rigidbody rb = money.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 forceDir = (targetATM.transform.forward + Vector3.up).normalized;
                    forceDir += Random.insideUnitSphere * 0.5f; // Hafif rastgelelik ekle
                    rb.AddForce(forceDir * 5f, ForceMode.Impulse);
                }
            }

            // Makinalı tüfek gibi teker teker çıkması için çok az bekle
            yield return new WaitForSeconds(0.1f);
        }
    }

    // Editörde menzili görmek için çizim yapar
    void OnDrawGizmosSelected()
    {
        if (targetATM != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetATM.transform.position, hitRange);
        }
    }
}