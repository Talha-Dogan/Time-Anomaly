using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game Settings")]
    public int targetMushroomCount = 10;
    public TextMeshProUGUI scoreText;

    public int currentScore = 0;
    private bool isShowingWarning = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddScore()
    {
        currentScore++;

        if (currentScore >= targetMushroomCount)
        {
            if (scoreText != null)
            {
                scoreText.text = "GO TO CAGE!";
                scoreText.color = Color.yellow;
            }
        }
        else
        {
            UpdateUI();
        }
    }

    public void WinGame()
    {
        StopAllCoroutines();
        if (scoreText != null)
        {
            scoreText.text = "MISSION COMPLETE!";
            scoreText.color = Color.green;
        }
        Debug.Log("You Won!");
    }

    // --- YENİ EKLENEN KISIM / NEW PART ---
    public void ShowMeteorObjective()
    {
        StopAllCoroutines(); // Eski uyarılar varsa dursun
        if (scoreText != null)
        {
            // "Ne oluyor? Git kontrol et!"
            scoreText.text = "WHAT WAS THAT? CHECK THE CRASH!";
            scoreText.color = Color.red; // Kırmızı yaparak tehlikeyi hissettirelim

            // İstersen yanıp sönme efekti de ekleyebilirsin ama şimdilik düz kırmızı yeterli
        }
        Debug.Log("New Objective: Check the meteor!");
    }
    // -------------------------------------

    public void ShowWarning()
    {
        if (isShowingWarning) return;
        StartCoroutine(WarningRoutine());
    }

    IEnumerator WarningRoutine()
    {
        isShowingWarning = true;

        if (scoreText != null)
        {
            scoreText.text = "NEED MUSHROOMS!";
            scoreText.color = Color.red;
        }

        yield return new WaitForSeconds(2f);

        if (currentScore < targetMushroomCount)
        {
            UpdateUI();
            if (scoreText != null) scoreText.color = Color.white;
        }

        isShowingWarning = false;
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString() + " / " + targetMushroomCount;
        }
    }
}