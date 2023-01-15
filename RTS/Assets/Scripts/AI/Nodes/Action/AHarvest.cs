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
            return State.Failure;

        int[] targetIds;

        if (context.HarvestedResource == _resourceType)
        {
            List<int> notHarvestingAllies = new List<int>();

            foreach (Character character in context.Characters)
                if (context.Leader.CurrentAction is not Harvest)
                    notHarvestingAllies.Add(character.ID);

            targetIds = notHarvestingAllies.ToArray();
        }
        else
        {
            context.HarvestedResource = _resourceType;

            targetIds = context.AllyIds;

            if (log)
                Debug.Log($"Harvest {_resourceType}");
        }


        if (targetIds.Length == 0)
            return State.Success;

        context.HarvestedResource = _resourceType;

        Vector2Int resourceCoords = context.KnownResources[_resourceType].Peek();

        context.Inputs.Add(TickInput.Harvest(resourceCoords, targetIds, context.Performer));

        return State.Success;
    }
}
