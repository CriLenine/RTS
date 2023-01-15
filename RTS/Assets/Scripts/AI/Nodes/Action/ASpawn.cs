using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;
using System;

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

        foreach (Building.Type type in Enum.GetValues(typeof(Building.Type)))
        {
            BuildingData buildingData = DataManager.GetBuildingData(type);

            foreach (ButtonDataHUDParameters parameter in buildingData.Actions)
            {
                if (parameter.ButtonData is not CharacterData characterData)
                    continue;

                if (characterData.Type == _characterType)
                    return Build(buildingData) ? State.Success : State.Failure;
            }
        }

        return State.Failure;
    }

    private bool Build(BuildingData data)
    {
        foreach (Resource.Amount cost in data.Cost)
            if (context.GetResourceAmount(cost.Type) < cost.Value)
                return false;

        Vector2Int? availableCoords = null;

        foreach (Vector2Int coords in SpiralForeach(context.StartCoords))
        {
            if (TileMapManager.TilesAvailableForBuild(data.Size, coords))
            {
                availableCoords = coords;

                break;
            }
        }

        if (availableCoords is null)
            return false;

        GameManager.dCoords = availableCoords.Value;

        if (log)
            Debug.Log($"Build {data.Type}");

        context.Inputs.Add(TickInput.NewBuild((int)data.Type, TileMapManager.TilemapCoordsToWorld(availableCoords.Value), context.AllyIds, context.Performer));

        return true;
    }

    private static IEnumerable<Vector2Int> SpiralForeach(Vector2Int startCoords, int maxSize = 100)
    {
        Vector2Int coords = startCoords;

        yield return coords;

        int sign = 1;

        for (int row = 1; row < maxSize; ++row)
        {
            for (int k = 0; k < row; ++k)
            {
                coords.x += sign;

                yield return coords;
            }

            for (int k = 0; k < row; ++k)
            {
                coords.x -= sign;

                yield return coords;
            }

            sign = -sign;
        }

        for (int k = 0; k < maxSize - 1; ++k)
        {
            coords.x += sign;

            yield return coords;
        }
    }
}
