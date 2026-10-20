using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns hazards and pickups off the right side of the screen.
/// The ocean alternates between calm stretches (random single hazards/pickups)
/// and sets (several whitewash walls back to back), like real surf.
/// Each prefab spawns at its own saved Y position, so a prefab's height
/// decides whether it appears above or below the water.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Where Things Spawn")]
    [Tooltip("X position just off the right edge of the screen.")]
    [SerializeField] private float spawnX = 11f;

    [Header("Timing")]
    [Tooltip("Seconds of calm at the very start of a run so the player can practice diving.")]
    [SerializeField] private float startDelay = 3f;
    [Tooltip("How long each calm stretch lasts between sets.")]
    [SerializeField] private float calmDuration = 8f;
    [Tooltip("Shortest time between spawns during a calm stretch.")]
    [SerializeField] private float calmMinGap = 1.2f;
    [Tooltip("Longest time between spawns during a calm stretch.")]
    [SerializeField] private float calmMaxGap = 2.2f;

    [Header("Sets")]
    [Tooltip("The whitewash wall prefab used for sets.")]
    [SerializeField] private GameObject wavePrefab;
    [Tooltip("Seconds between the SET INCOMING warning and the first wave.")]
    [SerializeField] private float setWarningTime = 1.5f;
    [Tooltip("Seconds between each wave in a set.")]
    [SerializeField] private float setWaveGap = 1.6f;
    [Tooltip("Fewest waves in a set.")]
    [SerializeField] private int setMinWaves = 3;
    [Tooltip("Most waves in a set at the start of a run.")]
    [SerializeField] private int setMaxWaves = 4;
    [Tooltip("Every this many meters paddled, sets can have one more wave.")]
    [SerializeField] private float distancePerExtraWave = 300f;
    [Tooltip("Breathing room after a set ends before the calm stretch starts spawning.")]
    [SerializeField] private float afterSetPause = 1.5f;

    [Header("Calm Stretch Spawns")]
    [Tooltip("Hazards and pickups that can appear during calm stretches.")]
    [SerializeField] private List<GameObject> calmSpawns = new List<GameObject>();

    /// <summary>Fires when a set is about to arrive (used later for the banner and horn).</summary>
    public event System.Action SetIncoming;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    /// <summary>Repeats forever: a calm stretch, then a set.</summary>
    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(startDelay);

        while (IsGameRunning())
        {
            yield return CalmStretch();
            yield return WaveSet();
        }
    }

    /// <summary>Spawns random hazards/pickups with gaps between them.</summary>
    private IEnumerator CalmStretch()
    {
        float endTime = Time.time + calmDuration;

        while (Time.time < endTime && IsGameRunning())
        {
            if (calmSpawns.Count > 0)
            {
                GameObject prefab = calmSpawns[Random.Range(0, calmSpawns.Count)];
                Spawn(prefab);
            }

            yield return new WaitForSeconds(Random.Range(calmMinGap, calmMaxGap));
        }
    }

    /// <summary>Warns the player, then sends several waves in a row.</summary>
    private IEnumerator WaveSet()
    {
        Debug.Log("SET INCOMING!");
        SetIncoming?.Invoke();
        yield return new WaitForSeconds(setWarningTime);

        // Sets get bigger the farther the surfer has paddled.
        int extraWaves = 0;
        if (GameManager.Instance != null)
        {
            extraWaves = Mathf.FloorToInt(GameManager.Instance.Distance / distancePerExtraWave);
        }
        int waveCount = Random.Range(setMinWaves, setMaxWaves + 1) + extraWaves;

        for (int i = 0; i < waveCount && IsGameRunning(); i++)
        {
            Spawn(wavePrefab);
            yield return new WaitForSeconds(setWaveGap);
        }

        yield return new WaitForSeconds(afterSetPause);
    }

    /// <summary>Creates a copy of the prefab at the right edge, at the prefab's own height.</summary>
    private void Spawn(GameObject prefab)
    {
        if (prefab == null)
        {
            return;
        }

        Vector3 position = new Vector3(spawnX, prefab.transform.position.y, 0f);
        Instantiate(prefab, position, prefab.transform.rotation);
    }

    /// <summary>True while a run is in progress (or if there's no GameManager, for testing).</summary>
    private bool IsGameRunning()
    {
        return GameManager.Instance == null || GameManager.Instance.IsPlaying;
    }
}
