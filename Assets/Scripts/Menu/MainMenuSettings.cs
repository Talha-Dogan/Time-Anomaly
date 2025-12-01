using UnityEngine;
using UnityEngine.UI;

public class MainMenuSettings : MonoBehaviour
{
    [Header("UI Elements")]
    public Toggle easyModeToggle;

    void Start()
    {
        if (easyModeToggle != null)
        {
            // Hafızadaki durumu çek (Varsayılan 0: Kapalı)
            int easyModeStatus = PlayerPrefs.GetInt("EasyMode", 0);

            // ESKİ YÖNTEM: easyModeToggle.isOn = true; (Bu, kodu tetikliyordu)

            // YENİ YÖNTEM: SetIsOnWithoutNotify
            // Bu komut sadece görseli değiştirir, "Tıklandı" kodunu çalıştırmaz!
            // Böylece oyun başlarken gereksiz yere kayıt yapmaz.
            easyModeToggle.SetIsOnWithoutNotify(easyModeStatus == 1);
        }
    }

    // Bu fonksiyon Toggle tıklandığında çalışır
    public void SetEasyMode(bool isOn)
    {
        if (isOn)
        {
            PlayerPrefs.SetInt("EasyMode", 1);
            Debug.Log("Easy Mode: ON (Kaydedildi)");
        }
        else
        {
            PlayerPrefs.SetInt("EasyMode", 0);
            Debug.Log("Easy Mode: OFF (Kaydedildi)");
        }

        PlayerPrefs.Save();
    }
}