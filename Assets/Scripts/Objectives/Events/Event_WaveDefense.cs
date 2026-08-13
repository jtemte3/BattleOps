using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers a wave defense event where all existing squads attack the extraction point
/// and new squads are continuously spawned to assault the position.
/// Place this at or near the extraction point. Call EndWaveDefense() when the player mounts the helicopter.
/// </summary>
public class Event_WaveDefense : MissionEvent
{
    [Header("Wave Defense Settings")]
    [Tooltip("Prefab for squads to spawn during the wave defense")]
    public AISquad waveSquadPrefab;
    public int maxWaveCount = 3;
    private int currentWave = 1;

    [Tooltip("Delay between each wave spawn in seconds")]
    public float waveDelay = 20f;

    [Tooltip("Radius around this point where new squads will spawn")]
    public float spawnRadius = 50f;

    [Tooltip("Minimum distance from this point to spawn squads")]
    public float minSpawnDistance = 30f;

    [Tooltip("If true, existing squads will be alerted to attack this position")]
    public bool alertExistingSquads = true;

    [Header("State")]
    [Tooltip("Reference to the extraction point (auto-assigned if null)")]
    public Transform extractionPoint;

    private List<AISquad> spawnedSquads = new List<AISquad>();
    private float waveTimer = 0f;
    private bool isWaveDefenseActive = false;

    private void Start()
    {
        // Auto-assign extraction point if not set
        if (extractionPoint == null)
        {
            extractionPoint = transform;
        }
    }

    public override void Engage()
    {
        if (isWaveDefenseActive && waveSquadPrefab != null)
        {
            if (currentWave <= maxWaveCount)
            {
                // Handle wave spawning timer
                waveTimer += Time.deltaTime;
                if (waveTimer >= waveDelay)
                {
                    waveTimer = 0f;
                    SpawnWave();
                }
            }
        }
        else
        {
            isWaveDefenseActive = true;
            waveTimer = 0f; // Reset timer on engage
            Debug.Log("[WaveDefense] Wave defense event activated!");

            // Alert all existing squads to attack the extraction point
            if (alertExistingSquads)
            {
                AlertAllExistingSquads();
            }

            // Spawn the first wave immediately
            if (waveSquadPrefab != null)
            {
                SpawnWave();
            }
        }
        
    }

    /// <summary>
    /// Ends the wave defense event. Call this when the player mounts the helicopter.
    /// </summary>
    public void EndWaveDefense()
    {
        if (!isWaveDefenseActive) return;

        isWaveDefenseActive = false;
        waveTimer = 0f;

        // End assault mode on all spawned squads
        foreach (AISquad squad in spawnedSquads)
        {
            if (squad != null)
            {
                squad.EndAssault();
            }
        }

        // Also end assault on existing squads that were alerted
        AISquad[] allSquads = FindObjectsByType<AISquad>();
        foreach (AISquad squad in allSquads)
        {
            if (squad != null && squad.isSquadAssaulting)
            {
                squad.EndAssault();
            }
        }

        Debug.Log("[WaveDefense] Wave defense ended!");
    }

    private void AlertAllExistingSquads()
    {
        AISquad[] allSquads = FindObjectsByType<AISquad>();

        foreach (AISquad squad in allSquads)
        {
            if (squad != null)
            {
                squad.ForceAttackTarget(extractionPoint);
            }
        }

        Debug.Log($"[WaveDefense] Alerted {allSquads.Length} existing squads to attack extraction point!");
    }

    private void SpawnWave()
    {
        if (waveSquadPrefab == null)
        {
            Debug.LogWarning("[WaveDefense] No wave squad prefab assigned!");
            return;
        }

        // Calculate spawn position in a ring around the extraction point
        Vector3 spawnPosition = GetRandomSpawnPosition();

        // Instantiate the squad
        AISquad newSquad = Instantiate(waveSquadPrefab, spawnPosition, Quaternion.identity);
        spawnedSquads.Add(newSquad);

        // Force the new squad to attack the extraction point immediately
        newSquad.ForceAttackTarget(extractionPoint);

        

        Debug.Log($"[WaveDefense] Wave {currentWave}: Spawned squad at {spawnPosition}");
        currentWave++;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // Generate a random angle
        float angle = Random.Range(0f, 360f);
        Vector3 direction = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            0f,
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );

        // Random distance within spawn radius
        float distance = Random.Range(minSpawnDistance, spawnRadius);

        // Calculate final spawn position
        Vector3 spawnPosition = extractionPoint.position + (direction * distance);

        // Keep Y at ground level (or adjust based on your terrain)
        spawnPosition.y = extractionPoint.position.y;

        return spawnPosition;
    }

    /// <summary>
    /// Destroys all squads spawned by this wave defense event.
    /// </summary>
    private void ClearSpawnedSquads()
    {
        foreach (AISquad squad in spawnedSquads)
        {
            if (squad != null)
            {
                Destroy(squad.gameObject);
            }
        }
        spawnedSquads.Clear();
    }

    private void OnDestroy()
    {
        // Clean up on destruction
        EndWaveDefense();
        ClearSpawnedSquads();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw spawn radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // Draw minimum spawn distance
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, minSpawnDistance);

        // Draw line to extraction point if different from this transform
        if (extractionPoint != null && extractionPoint != transform)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, extractionPoint.position);
        }
    }
#endif
}
