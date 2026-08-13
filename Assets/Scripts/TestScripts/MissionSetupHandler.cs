using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Handles dynamic mission setup: places the objective building in a random region,
/// regenerates all region buildings, and rebuilds the navmesh.
/// Call SetupMission() from LevelManager when the level loads.
/// </summary>
public class MissionSetupHandler : MonoBehaviour
{
    [Header("Objective Placement")]
    [Tooltip("The objective building already in the scene that will be moved to a random region")]
    public GameObject objectiveBuilding;

    [Header("Regions")]
    [Tooltip("All CellGridManager regions in the scene")]
    public CellGridManager[] regions;

    [Header("NavMesh")]
    [Tooltip("NavMeshSurface component on the Terrain object")]
    public NavMeshSurface navMeshSurface;

    [Header("Settings")]
    [Tooltip("How many attempts to find a valid position for the objective before falling back to region center")]
    public int objectivePlacementAttempts = 50;

    private bool isSetupComplete = false;

    /// <summary>
    /// Call this method from LevelManager to set up the mission.
    /// </summary>
    public void SetupMission()
    {
        if (objectiveBuilding == null)
        {
            Debug.LogError("MissionSetupHandler: objectiveBuilding is not assigned!");
            return;
        }

        if (regions == null || regions.Length == 0)
        {
            Debug.LogError("MissionSetupHandler: No regions assigned!");
            return;
        }

        if (navMeshSurface == null)
        {
            Debug.LogError("MissionSetupHandler: navMeshSurface is not assigned!");
            return;
        }

        isSetupComplete = false;

        // Start the async setup process
        StartCoroutine(SetupMissionCoroutine());
    }

    private IEnumerator SetupMissionCoroutine()
    {
        // Step 1: Place objective building in a random region
        PlaceObjectiveBuilding();

        // Step 2: Clear and regenerate all region buildings
        RegenerateAllRegionBuildings();

        // Wait one frame to ensure all meshes are fully initialized
        yield return null;

        // Step 3: Rebuild navmesh after a frame delay
        RebuildNavMesh();

        isSetupComplete = true;
        Debug.Log("MissionSetupHandler: Mission setup complete!");
    }

    /// <summary>
    /// Places the objective building in a random valid position within a randomly selected region.
    /// </summary>
    private void PlaceObjectiveBuilding()
    {
        // Pick a random region
        CellGridManager targetRegion = regions[Random.Range(0, regions.Length)];
        Debug.Log($"MissionSetupHandler: Placing objective in region at {targetRegion.transform.position}");

        // Find a valid open position in the region's cellMap
        Vector2Int targetGridPosition = FindValidPositionInRegion(targetRegion);

        // Calculate world position based on the region's anchor and cell size
        Vector3 worldPosition = new Vector3(
            (targetGridPosition.x * targetRegion.cellSize) + targetRegion.transform.position.x,
            targetRegion.transform.position.y,
            (targetGridPosition.y * targetRegion.cellSize) + targetRegion.transform.position.z
        );

        // Move the objective building to the new position
        objectiveBuilding.transform.position = worldPosition;
        objectiveBuilding.transform.SetParent(targetRegion.buildingParent?.transform ?? targetRegion.transform);

        // Mark the position and surrounding cells as taken in the region's cellMap
        MarkObjectivePositionAsTaken(targetRegion, targetGridPosition);

        Debug.Log($"MissionSetupHandler: Objective placed at grid position {targetGridPosition} (world: {worldPosition})");
    }

    /// <summary>
    /// Finds a valid open position in the given region's cellMap.
    /// </summary>
    private Vector2Int FindValidPositionInRegion(CellGridManager region)
    {
        // Try to find a random open position
        for (int i = 0; i < objectivePlacementAttempts; i++)
        {
            Vector2Int randomPos = VoronoiCityUtils.GetRandomPosition(region.cellMap);

            // Verify the position is actually open
            if (region.cellMap.ContainsKey(randomPos) && !region.cellMap[randomPos])
            {
                return randomPos;
            }
        }

        // Fallback: use the region's center position (or first available cell)
        Debug.LogWarning("MissionSetupHandler: Could not find open position after attempts. Using fallback position.");
        foreach (var kvp in region.cellMap)
        {
            if (!kvp.Value)
            {
                return kvp.Key;
            }
        }

        // Ultimate fallback: use region transform position as grid (0,0)
        return Vector2Int.zero;
    }

    /// <summary>
    /// Marks the objective building's position and surrounding cells as taken in the region's cellMap.
    /// </summary>
    private void MarkObjectivePositionAsTaken(CellGridManager region, Vector2Int gridPosition)
    {
        // Get the objective building's dimensions if it has a GridBuilding component
        GridBuilding objectiveGridBuilding = objectiveBuilding.GetComponent<GridBuilding>();
        Vector2Int dimensions = Vector2Int.one; // Default 1x1 if no GridBuilding component

        if (objectiveGridBuilding != null)
        {
            dimensions = objectiveGridBuilding.dimensions;
        }

        // Find all surrounding positions the objective will occupy
        List<Vector2Int> occupiedCells = VoronoiCityUtils.FindSurroundingPositions(
            gridPosition, dimensions.x, dimensions.y, false);

        // Mark all occupied cells as taken
        foreach (Vector2Int cell in occupiedCells)
        {
            if (region.cellMap.ContainsKey(cell))
            {
                region.cellMap[cell] = true;
            }
        }
    }

    /// <summary>
    /// Clears all generated buildings from every region and regenerates them.
    /// </summary>
    private void RegenerateAllRegionBuildings()
    {
        foreach (CellGridManager region in regions)
        {
            if (region == null) continue;

            // Remove existing generated buildings
            region.RemoveGeneratedBuildings();

            // Clear the grid positions so buildings can be placed again
            region.ClearGridPositions();

            // Regenerate the grid (rebuilds cellMap from perimeter)
            region.GenerateGrid();

            // Re-mark the objective position as taken if this region contains it
            if (region == GetRegionContainingObjective())
            {
                GridBuilding objBuilding = objectiveBuilding.GetComponent<GridBuilding>();
                Vector2Int objGridPos = GetObjectiveGridPosition(region);
                if (objGridPos != Vector2Int.zero || objectiveBuilding.transform.parent == region.transform)
                {
                    MarkObjectivePositionAsTaken(region, objGridPos);
                }
            }

            // Create new buildings for this region
            region.CreateBuildings();

            Debug.Log($"MissionSetupHandler: Regenerated buildings for region at {region.transform.position}");
        }
    }

    /// <summary>
    /// Rebuilds the navmesh using the NavMeshSurface component.
    /// </summary>
    private void RebuildNavMesh()
    {
        if (navMeshSurface != null)
        {
            Debug.Log("MissionSetupHandler: Rebuilding NavMesh...");

            // Build the navmesh
            //navMeshSurface.BuildNavMesh();
            navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
            Debug.Log("MissionSetupHandler: NavMesh rebuilt successfully.");
        }
    }

    /// <summary>
    /// Helper: Gets the region that currently contains the objective building.
    /// </summary>
    private CellGridManager GetRegionContainingObjective()
    {
        if (objectiveBuilding == null) return null;

        Transform parent = objectiveBuilding.transform.parent;
        if (parent == null) return null;

        foreach (CellGridManager region in regions)
        {
            if (region.transform == parent || region.buildingParent?.transform == parent)
            {
                return region;
            }
        }

        return null;
    }

    /// <summary>
    /// Helper: Calculates the grid position of the objective building within a region.
    /// </summary>
    private Vector2Int GetObjectiveGridPosition(CellGridManager region)
    {
        if (objectiveBuilding == null || region == null) return Vector2Int.zero;

        Vector3 localPos = objectiveBuilding.transform.position - region.transform.position;
        int gridX = Mathf.RoundToInt(localPos.x / region.cellSize);
        int gridZ = Mathf.RoundToInt(localPos.z / region.cellSize);

        return new Vector2Int(gridX, gridZ);
    }

    /// <summary>
    /// Returns true if the mission setup is complete.
    /// </summary>
    public bool IsSetupComplete()
    {
        return isSetupComplete;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only method to trigger setup from the Inspector.
    /// </summary>
    [ContextMenu("Setup Mission (Editor)")]
    public void SetupMissionEditor()
    {
        SetupMission();
    }
#endif
}
