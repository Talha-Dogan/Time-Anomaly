using UnityEngine;

public class FloatingText : MonoBehaviour
{
    #region Variables

    [Header("Scaling Settings (Büyüme/Küçülme)")]
    [Tooltip("How fast the text pulses.")]
    public float scaleSpeed = 3.0f;  // Büyüme hızı

    [Tooltip("How much larger/smaller it gets relative to initial size (e.g., 0.2 = +/- 20%).")]
    public float scaleAmount = 0.3f; // Ne kadar büyüyeceği

    // Private variables
    private Vector3 initialScale; // Başlangıç boyutunu saklamak için

    #endregion

    #region Unity Methods

    void Start()
    {
        // Store the starting scale so we oscillate around it
        initialScale = transform.localScale;
    }

    void Update()
    {
        // --- 1. SCALING LOGIC (Büyüyüp Küçülme) ---
        // We use Mathf.Sin because it smoothly oscillates between -1 and 1 over time.
        // Time.time ensures it keeps moving smoothly.
        float scaleFactor = Mathf.Sin(Time.time * scaleSpeed) * scaleAmount;

        // Apply the scale: Initial Scale * (1 + oscillating factor)
        // This makes it grow larger than 1 and smaller than 1 smoothly.
        transform.localScale = initialScale * (1f + scaleFactor);


        // --- 2. BILLBOARD LOGIC (Kameraya Bakma) ---
        // (This part is kept from your original script)
        if (Camera.main != null)
        {
            // Make the text face the camera while keeping its up vector aligned with the camera's up.
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                             Camera.main.transform.rotation * Vector3.up);
        }
    }

    #endregion
}