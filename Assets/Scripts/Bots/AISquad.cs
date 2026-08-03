using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages a group of AIEntity bots as a coordinated squad.
/// Handles shared threat detection, formation patrol movement, and squad alerting.
/// </summary>
public class AISquad : MonoBehaviour
{
    [Header("Squad Spawning")]
    [Tooltip("The prefab to use for squad members")]
    public GameObject entityPrefab;
    
    [Tooltip("Number of members to spawn in this squad")]
    public int squadSize = 4;
    
    [Tooltip("Radius to spread spawn positions to prevent clipping")]
    public float spawnRadius = 3.0f;

    [Header("Squad Configuration")]
    public List<AIEntity> squadMembers = new List<AIEntity>();
    
    [Tooltip("Patrol points specific to this squad. If empty, squad will idle.")]
    public Transform[] squadPatrolPoints;
    
    [Tooltip("How tightly the squad groups together during patrol")]
    public float formationRadius = 4.0f;
    
    [Tooltip("Random variance added to each member's destination to make formation look more natural")]
    public float destinationVariance = 2.0f;
    
    [Header("Squad State")]
    public Transform sharedTarget;
    public int currentPatrolIndex = 0;
    public bool isSquadAlerted = false;
    public bool isSquadSearching = false;

    // Lists to track roles during search
    private List<AIEntity> searchers = new List<AIEntity>();
    private List<AIEntity> suppressors = new List<AIEntity>();
    private Vector3 lastKnownTargetPosition;
    
    // Stores the random variance offset for each member for the current patrol point
    private Vector3[] memberVarianceOffsets;

    private void Awake()
    {
        // Spawn squad members if prefab and size are defined
        if (entityPrefab != null && squadSize > 0)
        {
            SpawnSquadMembers();
        }
        else if (squadMembers.Count == 0)
        {
            // Fallback to children if no prefab defined
            var entities = GetComponentsInChildren<AIEntity>();
            squadMembers.AddRange(entities);
        }
        
        memberVarianceOffsets = new Vector3[squadMembers.Count];
    }

    private void SpawnSquadMembers()
    {
        for (int i = 0; i < squadSize; i++)
        {
            // Calculate spawn position with slight random offset to prevent clipping
            Vector3 spawnOffset = Random.insideUnitSphere * spawnRadius;
            spawnOffset.y = 0; // Keep them on the ground plane relative to squad
            
            Vector3 spawnPosition = transform.position + spawnOffset;
            
            // Instantiate the entity
            GameObject newMemberGO = Instantiate(entityPrefab, spawnPosition, Quaternion.identity);
            AIEntity newMember = newMemberGO.GetComponent<AIEntity>();
            
            if (newMember != null)
            {
                squadMembers.Add(newMember);
                // Assign squad reference to the member so it can de-register itself on death
                newMember.squad = this;
                // Optionally parent them to the squad for organization
                newMemberGO.transform.SetParent(transform);
            }
        }
    }

    /// <summary>
    /// Removes a member from the squad (called when a member dies).
    /// </summary>
    public void RemoveMember(AIEntity member)
    {
        if (member == null) return;

        squadMembers.Remove(member);
        searchers.Remove(member);
        suppressors.Remove(member);

        // Regenerate variance offsets to keep indices aligned with the new squad size
        memberVarianceOffsets = new Vector3[squadMembers.Count];
        GenerateVarianceOffsets();
    }

    private void Update()
    {
        if (squadMembers.Count == 0) return;

        // 1. Check threats (Always highest priority - allows aborting search for new threats)
        CheckSharedThreats();

        if (isSquadAlerted)
        {
            HandleSquadCombat();
            return;
        }

        // 2. Handle Search State
        if (isSquadSearching)
        {
            HandleSquadSearch();
            return; // Skip patrol logic while searching
        }

        // 3. Handle Patrol
        HandleSquadPatrol();
    }

    /// <summary>
    /// Scans squad members for detected targets. If found, alerts the entire squad.
    /// If the shared target is lost, transitions the squad to search mode.
    /// </summary>
    private void CheckSharedThreats()
    {
        if (isSquadAlerted)
        {
            // Check if target is dead
            if (sharedTarget == null || sharedTarget.GetComponent<AIHealth>()?.GetCurrentHealth() <= 0)
            {
                isSquadAlerted = false;
                sharedTarget = null;
                return;
            }

            // Check if target is still detected by ANY squad member
            bool isTargetVisible = false;
            foreach (AIEntity member in squadMembers)
            {
                if (member.perception != null &&
                    member.perception.currentTarget == sharedTarget &&
                    member.perception.detectionState == DetectionState.Detected)
                {
                    isTargetVisible = true;
                    break;
                }
            }

            // If no one sees the target anymore, it got away -> Start Search
            if (!isTargetVisible)
            {
                isSquadAlerted = false;
                isSquadSearching = true;
                
                // Capture last known position before clearing targets
                if (sharedTarget != null)
                {
                    lastKnownTargetPosition = sharedTarget.position;
                }
                else if (squadMembers.Count > 0 && squadMembers[0].perception != null)
                {
                    lastKnownTargetPosition = squadMembers[0].perception.lastKnownPosition;
                }

                BroadcastSearch();
            }
        }
        else
        {
            // Check for new threats
            foreach (AIEntity member in squadMembers)
            {
                if (member.perception != null &&
                    member.perception.detectionState == DetectionState.Detected && 
                    member.perception.currentTarget != null)
                {
                    isSquadAlerted = true;
                    sharedTarget = member.perception.currentTarget;
                    BroadcastAlert();
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Forces all squad members to focus on the shared target immediately.
    /// </summary>
    private void BroadcastAlert()
    {
        // Clear search roles as we are entering combat
        searchers.Clear();
        suppressors.Clear();

        foreach (AIEntity member in squadMembers)
        {
            if (member.perception != null)
            {
                member.perception.currentTarget = sharedTarget;
                member.perception.lastKnownPosition = sharedTarget.position;
                member.perception.suspicionLevel = member.perception.detectionThreshold;
                member.perception.detectionState = DetectionState.Detected;
                member.SetState(AIState.Combat);
            }
            
            // FIX: Clear patrol destination immediately when entering combat
            // This prevents patrol logic from overriding combat movement
            AIPatrol patrol = member.GetComponent<AIPatrol>();
            if (patrol != null)
            {
                patrol.ClearDestination();
            }
        }
    }

    /// <summary>
    /// Transitions the squad to search mode, splitting into searchers and suppressors.
    /// </summary>
    private void BroadcastSearch()
    {
        searchers.Clear();
        suppressors.Clear();

        // Shuffle members to randomize roles
        List<AIEntity> shuffledMembers = new List<AIEntity>(squadMembers);
        shuffledMembers = shuffledMembers.OrderBy(x => Random.value).ToList();

        // Determine number of suppressors (1 to Count-1)
        int suppressorCount = 1;
        if (shuffledMembers.Count > 1)
        {
            suppressorCount = Random.Range(1, shuffledMembers.Count);
        }

        for (int i = 0; i < shuffledMembers.Count; i++)
        {
            AIEntity member = shuffledMembers[i];
            if (member.perception == null) continue;

            if (i < suppressorCount)
            {
                // Assign as Suppressor
                suppressors.Add(member);
                member.SetState(AIState.Suppress);
                member.perception.currentTarget = null;
                member.perception.lastKnownPosition = lastKnownTargetPosition;

                // FIX: Initialize suppression state immediately to prevent 
                // AIEntity from switching to Search before AICombat updates
                if (member.combat != null)
                {
                    member.combat.OverideSuppression();
                }
            }
            else
            {
                // Assign as Searcher
                searchers.Add(member);
                member.SetState(AIState.Search);
                member.perception.currentTarget = null;
                
                if (member.movement != null)
                {
                    member.movement.SetupSearchPath();
                }
            }
        }
    }

    /// <summary>
    /// Waits for all SEARCHERS to finish their search paths before regrouping.
    /// Suppressors will hold the line until the search is complete or a new threat is found.
    /// </summary>
    private void HandleSquadSearch()
    {
        bool allSearchersFinished = true;

        foreach (AIEntity searcher in searchers)
        {
            // If searcher is dead, ignore them
            if (searcher.currentState == AIState.Dead) continue;

            // Check if the searcher is still actively searching
            if (searcher.movement != null && searcher.movement.isSearching)
            {
                allSearchersFinished = false;
                break;
            }
        }

        // If all searchers are done, regroup the entire squad
        if (allSearchersFinished)
        {
            isSquadSearching = false;
            
            foreach (AIEntity member in squadMembers)
            {
                if (member.currentState == AIState.Dead) continue;

                if (member.movement != null)
                {
                    member.movement.isSearching = false;
                }
                
                // Clear patrol destinations and reset to patrol state
                AIPatrol patrol = member.GetComponent<AIPatrol>();
                if (patrol != null)
                {
                    patrol.ClearDestination();
                }

                member.SetState(AIState.Patrol);
            }
        }
    }

    /// <summary>
    /// Manages patrol movement. Calculates formation offsets so bots move together.
    /// </summary>
    private void HandleSquadPatrol()
    {
        if (squadPatrolPoints == null || squadPatrolPoints.Length == 0) return;

        Transform nextPoint = squadPatrolPoints[currentPatrolIndex];
        
        bool allMembersArrived = true;
        foreach (AIEntity member in squadMembers)
        {
            if (member.currentState == AIState.Dead) continue;

            AIPatrol patrol = member.GetComponent<AIPatrol>();
            if (patrol != null && !patrol.IsAtDestination())
            {
                allMembersArrived = false;
                break;
            }
        }

        if (allMembersArrived)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % squadPatrolPoints.Length;
            
            // Generate new random variance offsets for the new patrol point
            GenerateVarianceOffsets();
        }

        Vector3 centerPoint = squadPatrolPoints[currentPatrolIndex].position;
        
        for (int i = 0; i < squadMembers.Count; i++)
        {
            AIEntity member = squadMembers[i];
            if (member.currentState == AIState.Dead) continue;

            // FIX: Skip members that are in Combat state to prevent patrol from overriding combat movement
            if (member.currentState == AIState.Combat)
            {
                continue;
            }

            AIPatrol patrol = member.GetComponent<AIPatrol>();
            if (patrol != null)
            {
                Vector3 offset = GetFormationOffset(i, squadMembers.Count);
                Vector3 variance = (i < memberVarianceOffsets.Length) ? memberVarianceOffsets[i] : Vector3.zero;
                Vector3 destination = centerPoint + offset + variance;
                
                // Set patrol destination via the patrol component
                patrol.SetDestination(destination);
                
                // Ensure state is Patrol
                if (member.currentState != AIState.Patrol && member.currentState != AIState.Combat)
                {
                    member.SetState(AIState.Patrol);
                }
            }
        }
    }
    
    /// <summary>
    /// Generates random variance offsets for each squad member.
    /// </summary>
    private void GenerateVarianceOffsets()
    {
        for (int i = 0; i < memberVarianceOffsets.Length; i++)
        {
            memberVarianceOffsets[i] = new Vector3(
                Random.Range(-destinationVariance, destinationVariance),
                0,
                Random.Range(-destinationVariance, destinationVariance)
            );
        }
    }

    private Vector3 GetFormationOffset(int index, int totalMembers)
    {
        if (totalMembers <= 1) return Vector3.zero;
        float angle = (index / (float)totalMembers) * Mathf.PI * 2;
        return new Vector3(Mathf.Cos(angle) * formationRadius, 0, Mathf.Sin(angle) * formationRadius);
    }

    private void HandleSquadCombat()
    {
        if (sharedTarget != null)
        {
            foreach (AIEntity member in squadMembers)
            {
                if (member.perception != null)
                {
                    member.perception.currentTarget = sharedTarget;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (squadPatrolPoints == null || squadPatrolPoints.Length == 0) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < squadPatrolPoints.Length; i++)
        {
            Transform start = squadPatrolPoints[i];
            Transform end = squadPatrolPoints[(i + 1) % squadPatrolPoints.Length];
            if (start != null && end != null)
            {
                Gizmos.DrawLine(start.position, end.position);
            }
        }
    }
    
    private void OnDestroy()
    {
        // Clean up spawned members when squad is destroyed
        foreach (AIEntity member in squadMembers)
        {
            if (member != null)
            {
                Destroy(member.gameObject);
            }
        }
    }
}
