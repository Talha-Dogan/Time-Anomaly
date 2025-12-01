using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class MeteorCinematicTrigger : MonoBehaviour
{
    [Header("Sinematik Ayarlari")]
    public PlayableDirector timelineDirector; // Karakter animasyonunu (PickUp) yöneten Timeline
    public string sceneToLoad = "City";       // Gideceğimiz sahne

    [Header("Kamera Ayari")]
    public GameObject fpsVirtualCamera; // Kapatılacak olan FPS kamerası (Cinemachine/Virtual)
    public Transform cameraToControl;   // Yörüngeye girecek kamera (Genelde Main Camera)

    [Header("Model Yonetimi")]
    public GameObject playerGeometry;   // Oynadığın FPS karakterin kendisi/görseli (Gizlenecek)
    public GameObject cinematicModel;   // Timeline için koyduğun Cave Man (Açılacak)

    public Transform startCameraPos;    // İlk fotodaki başlangıç konumu (Boş obje)
    public Transform endCameraPos;      // İkinci fotodaki bitiş konumu (Boş obje)
    public Transform meteorTarget;      // Kameranın sürekli bakacağı hedef (Meteor)

    [Header("Zamanlama")]
    public float cutsceneDuration = 60.0f; // Dönüş süresi (1 dk)
    public float blinkSpeed = 0.2f;     // Göz kırpma hızı

    [Header("Kontrol Iptali")]
    public MonoBehaviour[] scriptsToDisable; // PlayerMovement, MouseLook vb.
    // public Rigidbody playerRb; // -> SİLDİK: Player'da Rigidbody olmadığı için gerek yok.

    // --- SİYAH EKRAN (Texture) ---
    private Texture2D blackTexture;
    private float fadeAlpha = 0f;

    private bool hasTriggered = false;

    void Start()
    {
        // 1x1 simsiyah bir doku oluşturuyoruz
        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();

        // Oyun başlar başlamaz Sinematik Cave Man'i gizle ki çift görünmesin
        if (cinematicModel != null) cinematicModel.SetActive(false);
    }

    void OnGUI()
    {
        if (fadeAlpha > 0)
        {
            GUI.color = new Color(0, 0, 0, fadeAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(PlaySequence());
        }
    }

    IEnumerator PlaySequence()
    {
        // 1. OYUNCU KONTROLLERİNİ KAPAT
        foreach (var script in scriptsToDisable) if (script != null) script.enabled = false;

        // 2. FPS KAMERASINI DEVRE DIŞI BIRAK
        if (fpsVirtualCamera != null) fpsVirtualCamera.SetActive(false);

        // 3. MODELLERİ DEĞİŞTİR (Karakter Değişimi)
        if (playerGeometry != null) playerGeometry.SetActive(false); // Gerçek oyuncuyu gizle
        if (cinematicModel != null) cinematicModel.SetActive(true);  // Sinematik Cave Man'i aç

        // 4. GÖZLERİ KAPAT (Blink - Fade Out)
        yield return StartCoroutine(FadeRoutine(0f, 1f, blinkSpeed));

        // --- EKRAN SİYAHKEN HAZIRLIK YAP ---

        // A) Kamerayı Başlangıç Pozisyonuna Işınla
        if (cameraToControl != null && startCameraPos != null)
        {
            cameraToControl.position = startCameraPos.position;
            cameraToControl.rotation = startCameraPos.rotation;
            if (meteorTarget != null) cameraToControl.LookAt(meteorTarget);
        }

        // B) Timeline'ı Başlat (Karakter animasyonu için)
        if (timelineDirector != null) timelineDirector.Play();

        // 5. GÖZLERİ AÇ (Blink - Fade In)
        yield return StartCoroutine(FadeRoutine(1f, 0f, blinkSpeed));

        // 6. KAMERA DÖNÜŞÜ VE BEKLEME ⏳
        // cutsceneDuration (60 sn) süresi boyunca kamerayı başlangıçtan bitişe yay çizerek götür
        float elapsed = 0f;
        Vector3 startPos = startCameraPos.position;
        Vector3 endPos = endCameraPos.position;

        while (elapsed < cutsceneDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / cutsceneDuration; // 0 ile 1 arasında geçen süre oranı

            if (cameraToControl != null && meteorTarget != null)
            {
                // Slerp (Küresel İnterpolasyon) ile iki nokta arasında yay çizerek hareket et
                // Bu, yarım daire benzeri sinematik bir geçiş sağlar.
                cameraToControl.position = Vector3.Slerp(startPos, endPos, t);

                // Sürekli meteora bakmasını sağla
                cameraToControl.LookAt(meteorTarget);
            }

            yield return null; // Bir sonraki kareyi bekle
        }

        // 7. GEÇİŞ
        SceneManager.LoadScene(sceneToLoad);
    }

    IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            fadeAlpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }
        fadeAlpha = endAlpha;
    }
}