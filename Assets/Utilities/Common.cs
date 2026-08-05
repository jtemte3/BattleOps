using System.Collections.Generic;
using UnityEngine;

public static class Common
{
    public static void AlertViaSound(Vector3 position, float alertRadius, AITeam team)
    {
        Collider[] colliders = Physics.OverlapSphere(position, alertRadius);

        List<AIEntity> entities = new();

        foreach (Collider collider in colliders)
        {
            if (collider.GetComponent<HitBox>() != null)
            {
                AIEntity hitboxEntity = collider.GetComponent<HitBox>().entity;

                if (!entities.Contains(hitboxEntity) && hitboxEntity.team != team)
                {
                    entities.Add(hitboxEntity);
                }
            }
        }

        foreach (AIEntity entity in entities)
        {
            if (entity.team == AITeam.Enemy)
            {
                if (entity.currentState != AIState.Suppress && entity.currentState != AIState.Search)
                {
                    if (entity.squad != null)
                    {
                        entity.squad.isSquadSearching = true;
                    }
                    else
                    {
                        // Lone entities must setup their own search path
                        if (entity.movement != null)
                        {
                            entity.movement.SetupSearchPath();
                        }
                    }

                    // Ensure the entity knows WHERE to search
                    if (entity.perception != null)
                    {
                        entity.perception.lastKnownPosition = position;
                    }

                    entity.currentState = AIState.Search;
                }
            }

            if (entity.team == AITeam.Neutral)
            {
                entity.currentState = AIState.Flee;
            }

        }
    }
}
