using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 1. Don't forget this library!

public class VoidReset : MonoBehaviour
{
    // Start and Update are not needed for this logic, you can delete them.

    // This function runs when something enters the Trigger area
    private void OnTriggerEnter(Collider other)
    {
        // 2. Check if the object falling is the "Player"
        if (other.CompareTag("Player"))
        {
            // 3. Restart the specific scene named "City"
            SceneManager.LoadScene("City");

            // Optional: Print a message to the console for testing
            Debug.Log("Player fell into the void! Restarting City scene...");
        }
    }
}