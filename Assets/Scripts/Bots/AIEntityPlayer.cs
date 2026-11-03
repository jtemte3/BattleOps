using System;
using UnityEngine;

public class AIEntityPlayer : AIEntity
{
    public override void InitializeSystems()
    {

    }

    public override void HandleStateLogic()
    {

    }

    public override void Die()
    {
        currentState = AIState.Dead;
        LogDeath();
    }
}
