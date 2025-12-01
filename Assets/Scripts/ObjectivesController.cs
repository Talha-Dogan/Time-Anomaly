using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectivesController : MonoBehaviour
{
    [Header("UI Elements")]
    // Drag and drop your Panel object here in the Inspector
    public GameObject objectivesPanel;

    void Start()
    {
        // Hide the panel when the game starts
        if (objectivesPanel != null)
        {
            objectivesPanel.SetActive(false);
        }
    }

    void Update()
    {
        // Show panel when Tab is pressed down
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            objectivesPanel.SetActive(true);
        }

        // Hide panel when Tab is released
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            objectivesPanel.SetActive(false);
        }
    }
}