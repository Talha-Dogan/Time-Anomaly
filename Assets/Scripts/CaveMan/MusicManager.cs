using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("Music Clips")]
    public AudioClip menuTheme;      // Main Menu Loop
    public AudioClip gameTheme;      // Game Loop
    public AudioClip startJingle;    // 3 sec intro

    [Header("Quit Sequence")]
    public AudioClip quitPanelTheme;  // Music while asking "Are you sure?"
    public AudioClip sadGoodbyeClip;  // Music AFTER clicking Yes (5 sec)
    public AudioClip cancelQuitTheme; // Music if they click NO

    [Header("Volume Settings")] // --- NEW SECTION ---
    [Range(0f, 1f)] public float gameVolume = 0.4f; // Default 40% volume for game
    [Range(0f, 1f)] public float defaultVolume = 1.0f; // Default 100% for menus

    private AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        PlayMenuMusic();
    }

    // --- PLAY FUNCTIONS ---

    public void PlayMenuMusic()
    {
        // Menu music plays at default (full) volume
        PlayMusic(menuTheme, false, defaultVolume);
    }

    public void PlayGameMusic()
    {
        // Game music plays at the specific lower volume
        PlayMusic(gameTheme, true, gameVolume);
    }

    public void PlayQuitPanelMusic()
    {
        PlayMusic(quitPanelTheme, true, defaultVolume);
    }

    public void PlayCancelQuitMusic()
    {
        PlayMusic(cancelQuitTheme, false, defaultVolume);
    }

    public void PlayStartJingle()
    {
        audioSource.Stop();
        audioSource.volume = defaultVolume; // Reset volume to full
        audioSource.loop = false;
        audioSource.clip = startJingle;
        audioSource.Play();
    }

    public void PlaySadGoodbye()
    {
        audioSource.Stop();
        audioSource.volume = defaultVolume; // Reset volume to full
        audioSource.loop = false;
        audioSource.clip = sadGoodbyeClip;
        audioSource.Play();
    }

    // --- HELPER (UPDATED) ---
    // Now takes a 'volume' parameter
    private void PlayMusic(AudioClip clip, bool loop, float volume)
    {
        if (audioSource.clip == clip) return;

        audioSource.Stop();
        audioSource.volume = volume; // Set the specific volume
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.Play();
    }
}