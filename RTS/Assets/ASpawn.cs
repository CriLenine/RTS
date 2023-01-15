using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

[System.Serializable]
public class ASpawn : ActionNode
{
    [SerializeField]
    private Character.Type _characterType;

    private CharacterData _characterData => DataManager.GetCharacterData(_characterType);

    protected override State OnUpdate()
    {
        foreach (Resource.Amount cost in _characterData.Cost)
            if (context.GetResourceAmount(cost.Type) < cost.Value)
                return State.Failure;

        foreach (Building building in context.Buildings)
        {
            if (building.Data.CanSpawnUnits && building.SpawnableTypes.Contains(_characterType))
            {
                context.Inputs.Add(TickInput.QueueSpawn((int)_characterType, building.ID, context.Leader.Position, context.Performer));

                if (log)
                    Debug.Log($"Spawn {_characterType}");

                return State.Success;
            }
        }

        return State.Failure;
    }
}
