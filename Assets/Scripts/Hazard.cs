using UnityEngine;

/// <summary>
/// Marks an object as dangerous. When the surfer touches it, they lose a life.
/// Every hazard (whitewash, jellyfish, rocks, etc.) gets this script.
/// </summary>
public class Hazard : MonoBehaviour
{
    [Tooltip("Remove this hazard after it hits the surfer (good for small things like trash). Leave off for big things like waves.")]
    [SerializeField] private bool destroyOnHit = false;

    /// <summary>Called by the surfer when this hazard hits them.</summary>
    public void OnHitSurfer()
    {
        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}