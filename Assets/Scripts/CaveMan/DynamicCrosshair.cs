using UnityEngine;
using UnityEngine.UI; // UI işlemleri için şart

public class DynamicCrosshair : MonoBehaviour
{
    #region Variables

    [Header("UI Settings")]
    [Tooltip("The Image component on your Canvas.")]
    public Image crosshairImage;

    [Header("Crosshair Sprites")]
    [Tooltip("The default crosshair (when looking at nothing).")]
    public Sprite normalSprite;

    [Tooltip("The crosshair when looking at a Mushroom.")]
    public Sprite enemySprite;

    [Header("Raycast Settings")]
    public float checkDistance = 10.0f; // Ne kadar uzağı görsün?

    private Transform camTransform;

    #endregion

    #region Unity Methods

    void Start()
    {
        // Cache the camera transform for performance
        if (Camera.main != null)
            camTransform = Camera.main.transform;

        // Start with normal sprite
        if (crosshairImage != null && normalSprite != null)
            crosshairImage.sprite = normalSprite;
    }

    void Update()
    {
        CheckTarget();
    }

    #endregion

    #region Custom Methods

    void CheckTarget()
    {
        if (crosshairImage == null || camTransform == null) return;

        Ray ray = new Ray(camTransform.position, camTransform.forward);
        RaycastHit hit;

        bool isTargetingMushroom = false;

        // 1. Işın bir şeye çarptı mı?
        if (Physics.Raycast(ray, out hit, checkDistance))
        {
            // 2. Çarptığı şey Mantar mı?
            MushroomAI mushroom = hit.collider.GetComponent<MushroomAI>();
            if (mushroom != null)
            {
                isTargetingMushroom = true;
            }
        }

        // 3. Duruma göre resmi değiştir
        if (isTargetingMushroom)
        {
            // Eğer zaten enemySprite takılı değilse değiştir (Performans için)
            if (crosshairImage.sprite != enemySprite)
                crosshairImage.sprite = enemySprite;
        }
        else
        {
            // Mantara bakmıyorsak normal hale dön
            if (crosshairImage.sprite != normalSprite)
                crosshairImage.sprite = normalSprite;
        }
    }

    #endregion
}