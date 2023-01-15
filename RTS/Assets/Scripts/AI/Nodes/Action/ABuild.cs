using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;

[System.Serializable]
public class ABuild : ActionNode
{
    [SerializeField]
    private Building.Type _buildingType;

    private BuildingData _buildingData => DataManager.GetBuildingData(_buildingType);

    protected override State OnUpdate()
    {
        foreach (Resource.Amount cost in _buildingData.Cost)
            if (context.GetResourceAmount(cost.Type) < cost.Value)
                return State.Failure;

        Vector2Int? availableCoords = null;

        foreach (Vector2Int coords in SpiralForeach(context.StartCoords))
        {
            if (TileMapManager.TilesAvailableForBuild(_buildingData.Size, coords))
            {
                availableCoords = coords;

                break;
            }
        }

        if (availableCoords is null)
            return State.Failure;

        GameManager.dCoords = availableCoords.Value;

        if (log)
            Debug.Log($"Build {_buildingData.Type}");

        context.Inputs.Add(TickInput.NewBuild((int)_buildingData.Type, TileMapManager.TilemapCoordsToWorld(availableCoords.Value), context.AllyIds, context.Performer));

        return State.Success;
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
