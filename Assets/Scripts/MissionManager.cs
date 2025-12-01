using UnityEngine;
using TMPro; // TextMeshPro

public class MissionManager : MonoBehaviour
{
    public static MissionManager instance;

    [Header("UI Components")]
    public GameObject missionPanel; // The panel containing missions
    public TextMeshProUGUI[] missionTexts; // List of mission texts

    [Header("Game Over Settings")]
    public GameObject winPanel; // Drag your "Thanks for Playing" panel here!

    [Header("Mission Status")]
    public bool[] isMissionComplete; // Mission status tracker

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initialize the status array based on text count
        isMissionComplete = new bool[missionTexts.Length];

        // Ensure panels are hidden at start
        if (missionPanel != null) missionPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
    }

    private void Update()
    {
        // --- HOLD TO SHOW LOGIC ---
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (missionPanel != null) missionPanel.SetActive(true);
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (missionPanel != null) missionPanel.SetActive(false);
        }
    }

    // Function called by CrystalPickup script
    public void CompleteMission(int missionIndex)
    {
        // Error check
        if (missionIndex < 0 || missionIndex >= missionTexts.Length)
        {
            Debug.LogWarning("Invalid Mission ID or missing text list!");
            return;
        }

        // If mission is already done, do nothing
        if (isMissionComplete[missionIndex]) return;

        // Mark mission as complete
        isMissionComplete[missionIndex] = true;

        // UI Update: Change "[ ]" to "[ X ]"
        TextMeshProUGUI currentText = missionTexts[missionIndex];
        currentText.text = currentText.text.Replace("[ ]", "[ X ]");
        currentText.color = Color.green;

        Debug.Log("Mission " + missionIndex + " Completed!");

        // --- CHECK WIN CONDITION ---
        CheckIfAllMissionsComplete();
    }

    private void CheckIfAllMissionsComplete()
    {
        // Check every mission in the list
        foreach (bool completed in isMissionComplete)
        {
            // If even ONE mission is false, stop here. Game is not over yet.
            if (!completed) return;
        }

        // If the loop finishes, it means all missions are TRUE.
        TriggerWinSequence();
    }

    private void TriggerWinSequence()
    {
        Debug.Log("ALL MISSIONS COMPLETED! GAME OVER.");

        // 1. Show the "Thanks for Playing" Panel
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        // 2. Unlock the Cursor (So player can click Exit/Restart buttons)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Optional: Stop the game time (Uncomment if you want to freeze game)
        // Time.timeScale = 0f;
    }
}