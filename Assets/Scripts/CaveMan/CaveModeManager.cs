using UnityEngine;

public class CaveModeManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject mushroomParent; // Bütün mantarların içinde olduğu Ana Obje

    void Start()
    {
        // 1. Hafızaya bak: Easy Mode açık mı? (Varsayılan 0: Kapalı)
        int isEasyModeOn = PlayerPrefs.GetInt("EasyMode", 0);

        if (mushroomParent != null)
        {
            if (isEasyModeOn == 1)
            {
                // Easy Mode AÇIK: Obje görünür olsun (Visible)
                mushroomParent.SetActive(true);
                Debug.Log("Kolay Mod Açık: Mantarlar Görünüyor!");
            }
            else
            {
                // Easy Mode KAPALI: Obje gizlensin (Invisible)
                mushroomParent.SetActive(false);
                Debug.Log("Kolay Mod Kapalı: Mantarlar Gizlendi.");
            }
        }
    }
}