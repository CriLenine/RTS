using TheKiwiCoder;
using UnityEngine;

[System.Serializable]
public class AAttackNearbyEnemies : ActionNode
{
    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        Debug.Log("Look for enemies");

        context.Inputs.Add(TickInput.Attack(context.EnemyIds[0], context.Enemies[0].Position, context.AllyIds, context.Performer));
        
        return State.Success;
    }
}
