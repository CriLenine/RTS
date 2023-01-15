using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

[System.Serializable]
public class AHarvest : ActionNode
{
    [SerializeField]
    private ResourceType _resourceType;

    protected override State OnUpdate()
    {
        if (context.KnownResources[_resourceType].Count == 0)
        {
            if (log)
                Debug.Log($"No {_resourceType} found");

            return State.Failure;
        }

        if (log)
            Debug.Log($"Harvest {_resourceType}");

        context.HarvestedResource = _resourceType;

        Vector2Int resourceCoords = context.KnownResources[_resourceType].Peek();

        context.Inputs.Add(TickInput.Harvest(resourceCoords, context.AllyIds, context.Performer));

        return State.Success;
    }
}
