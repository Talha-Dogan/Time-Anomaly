using UnityEngine;
using System.Collections.Generic;

public class PuzzleSlot : MonoBehaviour
{
    [Header("Settings")]
    public ColorPuzzleManager manager;

    // The list of colors this cube can cycle through (Yellow, Green, Blue, etc.)
    public List<Color> availableColors;

    [Header("Visuals")]
    public Material defaultMaterial; // DRAG YOUR EMPTY/DEFAULT MATERIAL HERE

    [Header("Interaction")]
    public float interactionDistance = 3.0f;
    public KeyCode interactionKey = KeyCode.E;

    // We start at -1 so the first press sets the index to 0 (the first color)
    private int currentColorIndex = -1;
    private Renderer myRenderer;
    private Transform playerTransform;

    void Start()
    {
        myRenderer = GetComponentInChildren<Renderer>();
        if (myRenderer == null)
        {
            Debug.LogError("Renderer component not found on the puzzle slot!", gameObject);
            return;
        }

        // 1. APPLY DEFAULT MATERIAL (Sets the starting visual state)
        if (defaultMaterial != null)
        {
            // Assigning the material instance forces the default look.
            myRenderer.material = defaultMaterial;
        }

        // 2. Find player (rest of Start() logic)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Check distance and Key Press
        if (Vector3.Distance(transform.position, playerTransform.position) <= interactionDistance)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                CycleColor();
            }
        }
    }

    /// <summary>
    /// Switches to the next color in the list.
    /// </summary>
    void CycleColor()
    {
        // Increment index, starts at 0 on the first press
        currentColorIndex++;

        // If we reach the end of the list, loop back to the first color (index 0)
        if (currentColorIndex >= availableColors.Count)
        {
            currentColorIndex = 0;
        }

        UpdateColorVisual();

        // Tell the manager to check if the combination is correct now
        manager.CheckPuzzle();
    }

    void UpdateColorVisual()
    {
        if (availableColors.Count > 0)
        {
            // Now we change the color property of the currently assigned material instance
            myRenderer.material.color = availableColors[currentColorIndex];
        }
    }

    // Helper function for the Manager to read this slot's color
    public Color GetCurrentColor()
    {
        if (currentColorIndex == -1 || availableColors.Count == 0) return Color.clear; // If not interacted yet, return clear/default
        return availableColors[currentColorIndex];
    }
}