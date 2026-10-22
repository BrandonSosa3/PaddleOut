using UnityEngine;

/// <summary>
/// Scrolls a background layer to the left to create a sense of movement.
/// Layers farther away use a smaller speed factor, so they move slower
/// (parallax), which makes the scene feel deep.
///
/// Two ways to loop forever:
///  - TiledStrip: a long repeating strip (sand, water). Jumps back by one tile
///    width so the pattern looks seamless.
///  - SingleObject: one item (a cloud, a palm tree). When it leaves the left
///    side of the screen, it reappears on the right.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxScroller : MonoBehaviour
{
    private enum LoopMode { TiledStrip, SingleObject }

    [Tooltip("TiledStrip for repeating ground/water strips, SingleObject for clouds, trees, etc.")]
    [SerializeField] private LoopMode loopMode = LoopMode.TiledStrip;
    [Tooltip("1 = moves at the same speed as obstacles. Smaller = farther away (slower).")]
    [SerializeField] private float speedFactor = 1f;

    [Header("SingleObject only")]
    [Tooltip("Once the object passes this X on the left, it wraps around.")]
    [SerializeField] private float leftEdgeX = -13f;
    [Tooltip("Where the object reappears on the right.")]
    [SerializeField] private float rightEdgeX = 13f;

    private float startX;
    private float tileWidth;

    private void Start()
    {
        startX = transform.position.x;
        // The width of one copy of the image, used to make the strip loop seamlessly.
        tileWidth = GetComponent<SpriteRenderer>().sprite.bounds.size.x * Mathf.Abs(transform.localScale.x);
    }

    private void Update()
    {
        // Match the ocean speed (which drops to 0 at game over). With no GameManager
        // (like on the title screen), drift at a gentle default speed.
        float oceanSpeed = GameManager.Instance != null ? GameManager.Instance.ScrollSpeed : 3f;

        Vector3 position = transform.position;
        position.x -= oceanSpeed * speedFactor * Time.deltaTime;

        if (loopMode == LoopMode.TiledStrip)
        {
            // After moving one full tile, jump back one tile. The pattern looks identical, so it's seamless.
            if (position.x <= startX - tileWidth)
            {
                position.x += tileWidth;
            }
        }
        else if (position.x < leftEdgeX)
        {
            position.x = rightEdgeX;
        }

        transform.position = position;
    }
}