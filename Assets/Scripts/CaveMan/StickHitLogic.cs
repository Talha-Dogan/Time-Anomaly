using UnityEngine;

public class StickHitLogic : MonoBehaviour
{
    #region Variables

    [Header("VFX Settings")]
    [Tooltip("Drag the impact VFX prefab from the asset pack here.")]
    public GameObject hitVfxPrefab;

    [Tooltip("How long the VFX should stay in the scene before being destroyed.")]
    public float vfxLifetime = 2.0f;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Detects when the stick enters another collider (trigger).
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 1. Ignore the Player itself
        if (other.CompareTag("Player")) return;

        // 2. Ignore other triggers to avoid hitting invisible zones
        if (other.isTrigger) return;

        // 3. Spawn the VFX directly
        SpawnImpactEffect(other);
    }

    #endregion

    #region Custom Methods

    private void SpawnImpactEffect(Collider other)
    {
        Vector3 hitPosition;

        // 1. Check if the collider is a MeshCollider
        MeshCollider meshCollider = other as MeshCollider;

        // 2. Robust Check: Non-convex MeshColliders and TerrainColliders don't support ClosestPoint.
        // We need this check to prevent errors/crashes.
        if ((meshCollider != null && !meshCollider.convex) || other is TerrainCollider)
        {
            // FALLBACK: Use ClosestPointOnBounds (less accurate but safe)
            hitPosition = other.ClosestPointOnBounds(transform.position);
        }
        else
        {
            // SAFE: Box, Sphere, Capsule, and Convex Mesh support this
            hitPosition = other.ClosestPoint(transform.position);
        }

        // 3. Instantiate the visual effect
        if (hitVfxPrefab != null)
        {
            GameObject vfx = Instantiate(hitVfxPrefab, hitPosition, Quaternion.identity);
            Destroy(vfx, vfxLifetime);
        }
    }

    #endregion
}