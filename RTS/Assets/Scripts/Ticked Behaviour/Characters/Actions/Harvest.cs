using Mono.Cecil;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Harvest : Action
{
    private Vector2Int _coords;
    private readonly Vector2Int _attractionPoint;
    private readonly Resource _resource;
    private float _duration;
    private bool _isHarvesting;
    public Harvest(Character character, Vector2Int attractionPoint, Resource resource) : base(character)
    {
        _attractionPoint = attractionPoint;
        _resource = resource;
    }

    protected override bool Update()
    {
        Peon peon = _character as Peon;

        if (_isHarvesting)
        {
            if (!ResourcesManager.Harvestable(_coords))
                _isHarvesting = false;

            else if (--_duration <= 0f)     // This peon just finished harvesting the tile
            {
                _isHarvesting = false;
                if (peon.CarriedResource.Value == 0 || peon.CarriedResource.Type != _resource.Data.Type) // The peon carries another type of resource, or nothing
                    peon.CarriedResource = new Resource.Amount(_resource.Data.Type);

                peon.CarriedResource = peon.CarriedResource.AddQuantity(_resource.Data.AmountPerHarvest);

                _resource.HarvestTile(_coords);
            }
            return false;
        }

        /* The harvested tile is depleted or the harvest didn't start */

        if (peon.CarriedResource.Value >= peon.Data.NMaxCarriedResources) // The peon needs to deposit his resources
        {
            Building building = GetNearestResourceStorer(_resource.Data.Type);
            if (building == null)
                Debug.LogError($"There is no resource storer of {_resource.Data.Type}.");

            List<Vector2> wayPointsToDeposit = LocomotionManager.RetrieveWayPoints(_character.Performer, _character, GetDepositPosition(building));
            if (wayPointsToDeposit == null)
                Debug.LogError("Pathfinding failed unexpectedly.");

            SetAction(new MoveHarvest(_character,  wayPointsToDeposit, (IResourceStorer)building, _resource));
            return true;
        }

        /* The peon can harvest something */

        Vector2Int? nextCoordsToHarvest = _resource.GetTileToHarvest(_character.Coords, _attractionPoint);
        if (nextCoordsToHarvest != null)
        {
            _duration = _resource.Data.HarvestingTime / 0.025f;
            _coords = nextCoordsToHarvest.Value;
            _isHarvesting = true;
            return false;
        }

        /* The peon needs to move to find resources */

        Vector2Int? newInputCoords = _resource.GetNext(_attractionPoint, _character.Coords, _character.Performer);
        if (newInputCoords != null)
        {
            Vector2Int? nextCoordsToGo = _resource.GetHarvestingPosition(newInputCoords.Value, _character.Coords, _character.Performer);
            if (nextCoordsToGo == null)
                return true;

            List<Vector2> wayPointsToGo = LocomotionManager.RetrieveWayPoints(_character.Performer, _character, nextCoordsToGo.Value);
            if (wayPointsToGo == null)
                Debug.LogError("Pathfinding failed unexpectedly.");

            SetAction(new Move(_character, wayPointsToGo));
        }

        return true;
    }

    private Building GetNearestResourceStorer(ResourceType type)
    {
        Building closestBuilding = null;
        float shortestDistance = float.MaxValue;
        foreach (Building building in GameManager.MyBuildings)
        {
            if (building.BuildCompletionRatio >= 1 && building is IResourceStorer resourceStorer)
            {
                if (!resourceStorer.StorableResources.Contains(type))
                    continue;

                closestBuilding ??= building;

                float distance = (building.transform.position - _character.transform.position).sqrMagnitude;
                if (distance < shortestDistance)
                {
                    closestBuilding = building;
                    shortestDistance = distance;
                }
            }
        }
        return closestBuilding;
    }

    private Vector2Int GetDepositPosition(Building building)
    {
        List<Vector2Int> availableTiles = new List<Vector2Int>();
        for (int outline = 1; outline <= building.Data.Outline + 1; ++outline)
        {
            for (int i = -outline; i <= outline; ++i)
            {
                for (int j = -outline; j <= outline; ++j)
                {
                    if (i != -outline && i != outline && j != -outline && j != outline)
                        continue;
                    Vector2Int tileCoords = building.Coords + new Vector2Int(i, j);
                    if (TileMapManager.GetLogicalTile(tileCoords)?.IsFree(_character.Performer) == true
                        && TileMapManager.FindPath(_character.Performer, _character.Coords, tileCoords)?.Count > 0)
                        availableTiles.Add(tileCoords);
                }
            }

            if (availableTiles.Count > 0)
                return Resource.FindClosestCoords(availableTiles, _character.Coords);
        }

        Debug.LogError("Cannot deposit to the building");
        return Vector2Int.zero;
    }
}
