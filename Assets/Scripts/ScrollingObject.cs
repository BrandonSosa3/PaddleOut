using UnityEngine;

/// <summary>
/// Moves an object to the left at the ocean's current speed, so it looks
/// like the surfer is paddling toward it. Deletes itself once off screen.
/// Used on every hazard and pickup.
/// </summary>
public class ScrollingObject : MonoBehaviour
{
    [Tooltip("Extra speed on top of the ocean speed (e.g. seagulls fly faster than the water moves).")]
    [SerializeField] private float extraSpeed = 0f;
    [Tooltip("Once the object passes this X position (off the left side of the screen), it is deleted.")]
    [SerializeField] private float destroyX = -12f;

    private void Update()
    {
        // Fall back to a default speed if there's no GameManager (handy for testing).
        float oceanSpeed = GameManager.Instance != null ? GameManager.Instance.ScrollSpeed : 5f;

        transform.position += Vector3.left * (oceanSpeed + extraSpeed) * Time.deltaTime;

        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }
}