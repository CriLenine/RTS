using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

[System.Serializable]
public class TNeedSoldier : ActionNode
{
    [SerializeField]
    private int _minSoliderCount;

    protected override State OnUpdate()
    {
        return context.SoldierCount < _minSoliderCount ? State.Success : State.Failure;
    }
}
