using TheKiwiCoder;
using UnityEngine;

[System.Serializable]
public class AAttackNearbyEnemies : ActionNode
{
    protected override State OnUpdate()
    {
        if (context.Leader.CurrentAction is Attack)
            return State.Success;

        if (log)
            Debug.Log("Attack enemies");

        context.Inputs.Add(TickInput.Attack(context.EnemyIds[0], context.Enemies[0].Position, context.AllyIds, context.Performer));
        
        return State.Success;
    }
}
