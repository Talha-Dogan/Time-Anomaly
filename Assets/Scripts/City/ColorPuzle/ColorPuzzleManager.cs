using UnityEngine;
using System.Collections.Generic;

public class ColorPuzzleManager : MonoBehaviour
{
    [Header("Configuration")]
    public List<Color> correctColorSequence;
    public PuzzleSlot[] puzzleSlots;

    [Header("Reward Object")]
    public GameObject fallingContainer;

    [Header("Physics Settings")]
    public float pushForce = 5f;  // Öne doğru itme gücü
    public float tiltForce = 2f;  // Öne doğru devrilme (dönme) gücü

    private bool isPuzzleSolved = false;

    public void CheckPuzzle()
    {
        if (isPuzzleSolved) return;

        if (puzzleSlots.Length != correctColorSequence.Count)
        {
            Debug.LogError("Error: Slot count mismatch!");
            return;
        }

        for (int i = 0; i < puzzleSlots.Length; i++)
        {
            if (puzzleSlots[i].GetCurrentColor() != correctColorSequence[i]) return;
        }

        Debug.Log("COLORS RIGHT! PUZZLE SOLVED!");
        OnPuzzleSolved();
    }

    private void OnPuzzleSolved()
    {
        isPuzzleSolved = true;

        if (fallingContainer != null)
        {
            // 1. Kutuyu Görünür Yap
            fallingContainer.SetActive(true);

            // 2. Fizik Motorunu Al
            Rigidbody rb = fallingContainer.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;

                // 3. İTMEK (Kutunun baktığı yöne doğru - Forward)
                // Bu kod kutuyu çatının kenarına doğru iter
                rb.AddForce(fallingContainer.transform.forward * pushForce, ForceMode.Impulse);

                // 4. EĞMEK / DÖNDÜRMEK (Tilt)
                // transform.right ekseninde döndürürsen öne doğru yuvarlanır
                rb.AddTorque(fallingContainer.transform.right * tiltForce, ForceMode.Impulse);
            }

            Debug.Log("Container pushed off the roof!");
        }
    }
}