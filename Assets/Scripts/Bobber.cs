using UnityEngine;

/// <summary>
/// Moves an object smoothly up and down in a wave pattern.
/// Used on jellyfish so the player has to time their dive around them.
/// Works alongside ScrollingObject (that handles left movement, this handles up/down).
/// </summary>
public class Bobber : MonoBehaviour
{
    [Tooltip("How far above and below its starting height it moves.")]
    [SerializeField] private float height = 1f;
    [Tooltip("How fast it bobs.")]
    [SerializeField] private float speed = 2f;

    private float startY;
    private float timeOffset;

    private void Start()
    {
        startY = transform.position.y;
        // Random starting point in the wave so jellyfish don't all bob in sync.
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        Vector3 position = transform.position;
        position.y = startY + Mathf.Sin(Time.time * speed + timeOffset) * height;
        transform.position = position;
    }
}