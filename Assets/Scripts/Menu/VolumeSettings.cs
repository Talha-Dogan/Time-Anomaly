using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [Header("Audio Configuration")]
    public AudioMixer mainAudioMixer; // Assign "MainMixer" here
    public Slider volumeSlider;       // Assign UI Slider here

    // FIXED NAMES (Must match Unity settings exactly)
    private const string MIXER_VOLUME = "MasterVolume";
    private const string PREF_VOLUME = "SavedVolume";

    private void Start()
    {
        // 1. Load saved volume or use 1 (Max volume) as default
        float savedVol = PlayerPrefs.GetFloat(PREF_VOLUME, 1f);

        // 2. Set Slider visual position
        if (volumeSlider != null)
            volumeSlider.value = savedVol;

        // 3. Apply volume immediately
        SetVolume(savedVol);
    }

    // Link this to Slider -> OnValueChanged
    public void SetVolume(float sliderValue)
    {
        // A. HARD LIMIT: Prevent 0 or Negative numbers (Causes crash)
        if (sliderValue < 0.0001f) sliderValue = 0.0001f;

        // B. LOGARITHMIC CONVERSION
        float dbValue = Mathf.Log10(sliderValue) * 20;

        // C. MUTE LOGIC (If slider is at bottom, silence it)
        if (sliderValue <= 0.001f) dbValue = -80f;

        // D. APPLY TO MIXER
        if (mainAudioMixer != null)
        {
            // Returns true if successful, false if parameter name is wrong
            bool result = mainAudioMixer.SetFloat(MIXER_VOLUME, dbValue);

            if (!result) Debug.LogError("ERROR: Could not find parameter 'MasterVolume' in Audio Mixer! Did you Expose it?");
        }

        // E. SAVE
        PlayerPrefs.SetFloat(PREF_VOLUME, sliderValue);
        PlayerPrefs.Save();
    }
}