using UnityEngine;
using UnityEngine.UI; // UI kullan?yorsan ekle

public class PlayerMushroomInventory : MonoBehaviour
{
    public int currentMushrooms = 0;
    public int maxMushrooms = 10;

    // Oyuncu 10 tane toplad? m? kontrolü
    public bool hasCollectedAll = false;

    [Header("UI Referanslar?")]
    public Text infoText; // Ekrana "Kafese Götür!" yazd?rmak için

    public void AddMushroom()
    {
        currentMushrooms++;

        // UI Güncellemesi (Örnek)
        if (infoText != null) infoText.text = "Mantarlar: " + currentMushrooms + "/" + maxMushrooms;

        // 10 Tane olduysa
        if (currentMushrooms >= maxMushrooms)
        {
            hasCollectedAll = true;
            Debug.Log("Çanta doldu! Üsse dön ve kafese b?rak.");

            if (infoText != null) infoText.text = "Çanta Dolu! KAFESE G?T!";
        }
    }

    // Kafese gelince bu fonksiyon çal??acak
    public void DeliverMushrooms()
    {
        if (hasCollectedAll)
        {
            Debug.Log("OYUN KAZANILDI!");
            // Buraya Win ekran?n? açan kodu yazabilirsin
            // Örnek: GameManager.Instance.WinGame();
        }
        else
        {
            Debug.Log("Yeterli mantar yok! Daha fazla topla.");
            if (infoText != null) infoText.text = "Yetersiz Mantar! (" + currentMushrooms + "/10)";
        }
    }
}