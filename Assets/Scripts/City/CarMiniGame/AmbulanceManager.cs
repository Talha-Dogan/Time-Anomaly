using UnityEngine;
using System.Collections;

public class AmbulanceManager : MonoBehaviour
{
    [Header("Game References")]
    public GameObject ambulance;

    [Header("Crystal Reward")]
    public GameObject crystalPrefab;
    public Transform windowSpawnPoint;
    public float throwForce = 5f;

    [Header("Timing Settings")]
    [Tooltip("Araba hareket ettikten kaç saniye sonra kristali atsın?")]
    public float throwDelay = 1.5f; // BURAYI ARTIRDIM (1.5 sn idealdir)

    [Header("Settings")]
    public int totalCars = 5;
    private int carsRemoved = 0;

    public void CarCleared()
    {
        carsRemoved++;
        Debug.Log("Car Removed! " + carsRemoved + "/" + totalCars);

        if (carsRemoved >= totalCars)
        {
            ReleaseAmbulance();
        }
    }

    void ReleaseAmbulance()
    {
        Debug.Log("All 5 cars cleared! Ambulance exiting...");

        if (ambulance != null)
        {
            // 1. Arabayı hareket ettir
            AmbulanceController controller = ambulance.GetComponent<AmbulanceController>();
            if (controller != null)
            {
                controller.DriveAway();
            }

            // 2. Fırlatma işlemini zamanlayıcı ile başlat
            StartCoroutine(ThrowCrystalRoutine());
        }
    }

    IEnumerator ThrowCrystalRoutine()
    {
        // Belirlenen süre kadar bekle (Araba hızlansın diye)
        yield return new WaitForSeconds(throwDelay);

        DropCrystal();
    }

    void DropCrystal()
    {
        if (crystalPrefab != null && windowSpawnPoint != null)
        {
            GameObject newCrystal = Instantiate(crystalPrefab, windowSpawnPoint.position, Quaternion.identity);

            // Büyüme scripti varsa çalışır...

            Rigidbody rb = newCrystal.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Harekete başladığı için ileri doğru ivmesi var,
                // kristali atarken arabanın arkasına düşmemesi için biraz daha sert fırlatıyoruz.
                rb.AddForce(windowSpawnPoint.right * throwForce + Vector3.up * 3f, ForceMode.Impulse);

                rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
            }
        }
    }
}