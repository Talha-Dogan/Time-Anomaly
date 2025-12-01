using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsPanel;     // Drag Settings Panel here
    public GameObject mainButtonsGroup;  // Drag Main Menu Buttons here

    // --- BACK BUTTON LOGIC ---
    public void BackToMenu()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainButtonsGroup != null) mainButtonsGroup.SetActive(true);

        Debug.Log("Returned to Main Menu");
    }

    // --- GRAPHICS CONTROL ---
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // --- GAMEPLAY CONTROL ---
    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        PlayerPrefs.Save();
    }
}