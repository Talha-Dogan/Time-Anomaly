using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Ayarları")]
    public float mouseSensitivity = 100f;

    [Header("Bağlantılar")]
    public Transform playerBody; // Karakterin ana objesi

    private float xRotation = 0f; // Yukarı/Aşağı bakış açısı

    void Start()
    {
        // Oyuna başlarken fareyi ekranın ortasına kilitle ve gizle
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Fare girdilerini al
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // --- Yukarı/Aşağı Bakış (Kamera Rotasyonu) ---
        // mouseY'ı xRotation'dan çıkarıyoruz (ters çalışır)
        xRotation -= mouseY;

        // Kameranın 180 derece dönüp ters dönmesini engellemek için
        // bakış açısını -90 ve +90 derece arasında kilitliyoruz (clamp)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Sadece kameranın YUKARI/AŞAĞI rotasyonunu uygula
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);


        // --- Sağa/Sola Bakış (Karakter Rotasyonu) ---
        // Bütün karakter bedenini 'Y' ekseninde (yani kendi etrafında) döndür
        playerBody.Rotate(Vector3.up * mouseX);
    }
}