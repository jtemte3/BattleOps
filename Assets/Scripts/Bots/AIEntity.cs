using UnityEngine;

public class AIEntity : MonoBehaviour
{
    [Header("Team")]
    public AITeam team = AITeam.Enemy;

    [Header("State")]
    public AIState currentState = AIState.Idle;

    [HideInInspector] public AIHealth health;
    [HideInInspector] public AIMovement movement;
    [HideInInspector] public AIPerception perception;
    [HideInInspector] public AICombat combat;

    private void Awake()
    {
        if (team != AITeam.Player)
        {
            health = GetComponent<AIHealth>();
            movement = GetComponent<AIMovement>();
            perception = GetComponent<AIPerception>();
            combat = GetComponent<AICombat>();

            InitializeSystems();
        }
    }

    private void Update()
    {
        if (currentState == AIState.Dead || team == AITeam.Player)
            return;

        HandleStateLogic();
    }

    private void InitializeSystems()
    {
        if (health != null)
            health.Initialize(this);

        if (movement != null)
            movement.Initialize(this);

        if (perception != null)
            perception.Initialize(this);

        if (combat != null)
            combat.Initialize(this);
    }

    private void HandleStateLogic()
    {
        switch (currentState)
        {
            case AIState.Idle:
            case AIState.Patrol:

                if (perception.currentTarget != null)
                {
                    SetState(AIState.Combat);
                }

                break;

            case AIState.Combat:

                if (perception.currentTarget == null)
                {
                    SetState(AIState.Search);
                }

                break;

            case AIState.Search:

                if (perception.currentTarget != null)
                {
                    SetState(AIState.Combat);
                }

                break;
        }
    }

    public void SetState(AIState newState)
    {
        currentState = newState;
    }

    public void Die()
    {
        currentState = AIState.Dead;

        movement.enabled = false;
        perception.enabled = false;
        combat.enabled = false;
    }
}