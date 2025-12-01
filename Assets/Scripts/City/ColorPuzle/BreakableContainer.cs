using UnityEngine;

public class BreakableContainer : MonoBehaviour
{
    [Header("Reward Settings")]
    public GameObject crystalPrefab; // İçinden çıkacak kristal
    public GameObject breakEffect;   // (Opsiyonel) Kırılma efekti/toz

    [Header("Physics Settings")]
    public float impactForceThreshold = 2f; // Ne kadar sert çarparsa kırılsın?
    public float crystalPopForce = 5f;      // Kristal ne kadar zıplasın?

    private bool hasBroken = false;

    // Yere veya başka bir şeye çarptığında çalışır
    private void OnCollisionEnter(Collision collision)
    {
        // Eğer zaten kırıldıysa veya çarpma çok yavaşsa (sürtünme gibi) çalışma
        if (hasBroken || collision.relativeVelocity.magnitude < impactForceThreshold) return;

        BreakAndSpawn();
    }

    void BreakAndSpawn()
    {
        hasBroken = true;

        // 1. Kristali Oluştur
        if (crystalPrefab != null)
        {
            GameObject gem = Instantiate(crystalPrefab, transform.position + Vector3.up, Quaternion.identity);

            // Kristale fırlama efekti ver
            Rigidbody gemRb = gem.GetComponent<Rigidbody>();
            if (gemRb != null)
            {
                gemRb.AddForce(Vector3.up * crystalPopForce, ForceMode.Impulse);
                gemRb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
            }
        }

        // 2. Kırılma Efekti (Toz, patlama vs.)
        if (breakEffect != null)
        {
            Instantiate(breakEffect, transform.position, Quaternion.identity);
        }

        // 3. Bu kutuyu yok et
        Destroy(gameObject);

        // Ses efekti eklemek istersen: AudioSource.PlayClipAtPoint(...)
    }
}