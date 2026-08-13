using UnityEngine;

public class AIEntity : MonoBehaviour
{
    [Header("Team")]
    public AITeam team = AITeam.Enemy;

    [Header("State")]
    public AIState currentState = AIState.Idle;
    public bool canMove = true;

    [Header("AI Target")]
    public Transform target;

    [Header("Squad")]
    [Tooltip("Optional reference to the AISquad this entity belongs to. If assigned, the entity will de-register itself upon death.")]
    public AISquad squad;

    public AIHealth health;
    public AIMovement movement;
    public AIPerception perception;
    public AICombat combat;
    public AICosmetics cosmetics;

    public GameObject weapon;

    private void Awake()
    {
        if (team != AITeam.Player)
        {
            if (health == null)
            {
                health = GetComponent<AIHealth>();
            }
            if (movement == null && canMove == true)
            {
                movement = GetComponent<AIMovement>();
            }
            if (perception == null)
            {
                perception = GetComponent<AIPerception>();
            }
            if (combat == null)
            {
                combat = GetComponent<AICombat>();
            }
            if (cosmetics == null)
            {
                cosmetics = GetComponent<AICosmetics>();
            }

            InitializeSystems();
        }
    }

    private void Update()
    {
        if (currentState == AIState.Dead || team == AITeam.Player)
        {
            return;
        }

        HandleStateLogic();
    }

    private void InitializeSystems()
    {
        if (health != null)
        {
            health.Initialize(this);
        }

        if (movement != null)
        {
            movement.Initialize(this);
        }

        if (perception != null)
        {
            perception.Initialize(this);
        }

        if (combat != null)
        {
            combat.Initialize(this);
        }

        if (cosmetics != null)
        {
            cosmetics.Initialize(this);
        }
    }

    private void HandleStateLogic()
    {
        //Don't make a case for dead. The AIHealth script will trigger that function
        switch (currentState)
        {
            case AIState.Idle:
                if (perception.currentTarget != null && perception.detectionState == DetectionState.Detected)
                {
                    SetState(AIState.Combat);
                }

                if (canMove)
                {
                    if (perception.currentTarget != null && perception.detectionState == DetectionState.Suspicious)
                    {
                        movement.SetupSearchPath();
                        SetState(AIState.Search);
                    }
                }
                break;

            case AIState.Patrol:

                if (perception.currentTarget != null && perception.detectionState == DetectionState.Detected)
                {
                    SetState(AIState.Combat);
                }

                /*if (perception.currentTarget != null && perception.detectionState == DetectionState.Suspicious)
                {
                    movement.SetupSearchPath();
                    SetState(AIState.Search);
                }*/
                break;

            case AIState.Combat:

                if (perception.currentTarget == null)
                {
                    SetState(AIState.Suppress);
                }
                break;

            case AIState.Suppress:
                
                if (perception.currentTarget != null && perception.detectionState == DetectionState.Detected)
                {
                    SetState(AIState.Combat);
                }

                if (combat.isSuppressing == false)
                {
                    if (canMove)
                    {
                        movement.SetupSearchPath();
                        SetState(AIState.Search);
                    }
                }
                break;

            case AIState.Search:

                if (perception.currentTarget != null && perception.detectionState == DetectionState.Detected)
                {
                    SetState(AIState.Combat);
                }

                if (movement.isSearching == false)
                {
                    SetState(AIState.Idle);
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
        perception.detectionState = DetectionState.None;
        combat.enabled = false;

        weapon.SetActive(false);
        // De-register from squad if assigned
        if (squad != null)
        {
            squad.RemoveMember(this);
        }

        //Destroy(this.gameObject);
    }
}
