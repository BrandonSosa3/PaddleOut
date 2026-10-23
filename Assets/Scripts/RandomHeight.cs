using UnityEngine;

/// <summary>
/// Moves this object to a random height when it spawns.
/// Used on seagulls so some skim low over the water and others fly high
/// in the sky, where they're a danger during kite rides.
/// </summary>
public class RandomHeight : MonoBehaviour
{
    [SerializeField] private float minY = 1.2f;
    [SerializeField] private float maxY = 4.2f;

    private void Start()
    {
        Vector3 position = transform.position;
        position.y = Random.Range(minY, maxY);
        transform.position = position;
    }
}