using UnityEngine;

public class WeaponCollision : MonoBehaviour
{
    [Header("Settings")]
    public Transform weaponTransform;
    public float maxRetraction = 0.6f;
    public float checkDistance = 1.5f;
    public LayerMask obstacleLayers;
    public float smoothSpeed = 15f; // Hızı biraz arttırdık, tepki süresi iyileşsin diye
    public float detectionRadius = 0.25f; // Yarıçapı çok az küçülttük, çok hassas olmasın

    private Vector3 originalLocalPos;
    private Vector3 targetPos;
    private Vector3 currentRetractionVelocity; // Daha pürüzsüz geçiş için (SmoothDamp)

    void Start()
    {
        // Eğer weaponTransform boşsa, bu scriptin takılı olduğu objeyi (kendisini) ata.
        if (weaponTransform == null)
        {
            weaponTransform = transform;
        }
    }

    void Update()
    {
        Vector3 direction = transform.forward;
        float finalRetraction = 0f; // 0 = normal, 1 = tam geri çekilmiş

        // 1. ACİL DURUM KONTROLÜ: Başlangıç noktamız zaten duvarın içinde mi?
        if (Physics.CheckSphere(transform.position, detectionRadius, obstacleLayers))
        {
            // Evet, duvarın içindeyiz! Hiç hesap yapma, direkt tam geri çek.
            finalRetraction = 1.0f;
        }
        // 2. NORMAL KONTROL: Önümüzde duvar var mı?
        else if (Physics.SphereCast(transform.position, detectionRadius, direction, out RaycastHit hit, checkDistance, obstacleLayers))
        {
            // Duvara yaklaşıyoruz, mesafeye göre hesapla
            float distanceToWall = hit.distance;
            // Mesafeyi ters orantılı çevir (Yakınsa 1, uzaksa 0)
            finalRetraction = Mathf.Clamp01(1.0f - (distanceToWall / checkDistance));
        }

        // Hedef pozisyonu hesapla
        Vector3 retractionVector = new Vector3(0, 0, -finalRetraction * maxRetraction);
        Vector3 desiredPos = originalLocalPos + retractionVector;

        // Lerp yerine SmoothDamp kullandım, titremeyi (jitter) tamamen yok eder
        weaponTransform.localPosition = Vector3.SmoothDamp(
            weaponTransform.localPosition,
            desiredPos,
            ref currentRetractionVelocity,
            1f / smoothSpeed
        );
    }

    void OnDrawGizmos()
    {
        // Görselleştirme
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius); // Başlangıç kontrolü

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * checkDistance, detectionRadius); // Bitiş noktası
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * checkDistance);
    }
}