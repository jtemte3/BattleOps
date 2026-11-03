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
                AIEntityBot botentity = (AIEntityBot)entity;
                if (entity.currentState != AIState.Suppress && entity.currentState != AIState.Combat)
                {
                    if (botentity.squad != null)
                    {
                        botentity.squad.isSquadSearching = true;
                    }
                    else
                    {
                        // Lone entities must setup their own search path
                        if (botentity.perception != null)
                        {
                            botentity.perception.AddSuspicion(BotDetectionAlgorithm(botentity, sourceEntity, autoDetectionDistance), sourceEntity.transform);
                        }
                    }
                }
            }
        }

        //Add AIEntityCivilian here
    }

    private static float BotDetectionAlgorithm(AIEntityBot entity, AIEntity sourceEntity, float autoDetectionDistance)
    {
        float distanceBetweenEntities = Vector3.Distance(entity.transform.position, sourceEntity.transform.position);

        float percentDetection = (autoDetectionDistance / distanceBetweenEntities) * 100f;

        return entity.perception.suspicionIncreaseRate * percentDetection;
    }
}
