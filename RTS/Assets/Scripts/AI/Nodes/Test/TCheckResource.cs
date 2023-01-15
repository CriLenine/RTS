using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

[System.Serializable]
public class TCheckResource : ActionNode
{
    [SerializeField]
    private ResourceType _resourceType;

    [SerializeField]
    private int _minAmount = 500;

    [SerializeField]
    private int _targetAmount = 1000;

    protected override State OnUpdate()
    {
        if (context.GetResourceAmount(_resourceType) > _targetAmount)
            return State.Failure;

        if (context.Leader.CurrentAction is Harvest && context.HarvestedResource == _resourceType)
            return State.Success;

        return context.GetResourceAmount(_resourceType) < _minAmount ? State.Success : State.Failure;
    }
}
