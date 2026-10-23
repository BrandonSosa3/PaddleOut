using UnityEngine;

/// <summary>
/// Marks an object as dangerous. When the surfer touches it, they lose a life.
/// Every hazard (whitewash, pufferfish, rocks, etc.) gets this script.
/// A hazard can also have its own extra sound (like the pufferfish zap).
/// </summary>
public class Hazard : MonoBehaviour
{
    [Tooltip("Remove this hazard after it hits the surfer (good for small things). Leave off for big things like waves.")]
    [SerializeField] private bool destroyOnHit = false;
    [Tooltip("Optional extra sound played when this hazard hits the surfer.")]
    [SerializeField] private AudioClip hitSound;

    /// <summary>Called by the surfer when this hazard hits them.</summary>
    public void OnHitSurfer()
    {
        if (hitSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(hitSound);
        }

        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}