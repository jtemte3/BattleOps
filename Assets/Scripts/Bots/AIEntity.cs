using UnityEngine;

public abstract class AIEntity : MonoBehaviour
{
    [Header("Team")]
    public AITeam team = AITeam.Enemy;

    [Header("AI Target")]
    public Transform target;

    [Header("State")]
    public AIState currentState = AIState.Idle;
    public bool canMove = true;

    private void Awake()
    {
        InitializeSystems();
    }

    public void SetState(AIState newState)
    {
        currentState = newState;
    }
    public abstract void InitializeSystems();
    public abstract void HandleStateLogic();
    public abstract void Die();
    public void LogDeath()
    {
        MissionStatsTracker tracker = FindAnyObjectByType<MissionStatsTracker>();

        if (tracker != null)
        {
            switch (team)
            {
                case AITeam.Enemy:
                    tracker.IncrementBaddieDeath();
                    break;
                case AITeam.Ally:
                    tracker.IncrementSquadKilled();
                    break;
                case AITeam.Neutral:
                    tracker.IncrementCivilianDeath();
                    break;
                case AITeam.Player:
                    tracker.IncrementSquadKilled();
                    break;
            }
        }
    }

}
