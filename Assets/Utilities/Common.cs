using System.Collections.Generic;
using UnityEngine;

public static class Common
{
    public static void AlertViaSound(Vector3 position, float alertRadius, AIEntity sourceEntity)
    {
        float autoDetectionDistance = 5;
        Collider[] colliders = Physics.OverlapSphere(position, alertRadius);

        List<AIEntity> entities = new();

        foreach (Collider collider in colliders)
        {
            if (collider.GetComponent<HitBox>() != null)
            {
                AIEntity hitboxEntity = collider.GetComponent<HitBox>().entity;

                if (!entities.Contains(hitboxEntity) && hitboxEntity.team != sourceEntity.team)
                {
                    entities.Add(hitboxEntity);
                }
            }
        }

        foreach (AIEntity entity in entities)
        {
            if (entity.team == AITeam.Enemy)
            {
                if (entity.currentState != AIState.Suppress && entity.currentState != AIState.Combat)
                {
                    if (entity.squad != null)
                    {
                        entity.squad.isSquadSearching = true;
                    }
                    else
                    {
                        // Lone entities must setup their own search path
                        if (entity.perception != null)
                        {
                            entity.perception.AddSuspicion(DetectionAlgorithm(entity, sourceEntity, autoDetectionDistance), sourceEntity.transform);
                        }
                    }
                }
            }

            if (entity.team == AITeam.Neutral)
            {
                entity.currentState = AIState.Flee;
            }

        }
    }

    private static float DetectionAlgorithm(AIEntity entity, AIEntity sourceEntity, float autoDetectionDistance)
    {
        float distanceBetweenEntities = Vector3.Distance(entity.transform.position, sourceEntity.transform.position);

        float percentDetection = (autoDetectionDistance / distanceBetweenEntities) * 100f;

        return entity.perception.suspicionIncreaseRate * percentDetection;
    }
}
