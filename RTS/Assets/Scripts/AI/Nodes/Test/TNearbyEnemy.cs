using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

[System.Serializable]
public class TNearbyEnemy : ActionNode
{
    protected override State OnUpdate()
    {
        return context.Enemies.Count > 0 ? State.Success : State.Failure;
    }
}
