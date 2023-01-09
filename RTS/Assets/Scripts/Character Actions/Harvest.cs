using System.Collections.Generic;
using UnityEngine;

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
        if (_isHarvesting)
        {
            if (!ResourcesManager.Harvestable(_coords))
                _isHarvesting = false;

            else if (--_duration <= 0f)     // This peon just finished harvesting the tile
            {
                _isHarvesting = false;
                if (_character.HarvestedResource.Value == 0 || _character.HarvestedResource.Type != _resource.Data.Type) // The peon carries another type of resource, or nothing
                    _character.SetResource(new Resource.Amount(_resource.Data.Type));

                _character.HarvestedResource = _character.HarvestedResource.AddQuantity(_resource.Data.AmountPerHarvest);

                _resource.HarvestTile(_coords, _character.Data.AmountGetPerHarvest);
            }
            return false;
        }

        /* The harvested tile is depleted or the harvest didn't start */

        if (_character.HarvestedResource.Value >= _character.Data.MaxCarriedResources) // The peon needs to deposit his resources
        {
            Building building = GetNearestResourceStorer(_resource.Data.Type);
            if (building == null)
                Debug.LogError($"There is no resource storer of {_resource.Data.Type}.");

            List<Vector2> wayPointsToDeposit = LocomotionManager.RetrieveWayPoints(_character.Performer, _character, building.GetClosestOutlinePosition(_character));
            if (wayPointsToDeposit == null)
                Debug.LogError("Pathfinding failed unexpectedly.");

            SetAction(new MoveHarvest(_character, wayPointsToDeposit, building, _resource));
            return true;
        }

        /* The peon can harvest something */

        Vector2Int? nextCoordsToHarvest = _resource.GetTileToHarvest(_character.Coords, _attractionPoint);
        if (nextCoordsToHarvest != null)
        {
            _duration = _resource.Data.HarvestingTime / _character.Data.HarvestingSpeed / 0.025f;
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
            if (building.BuildCompletionRatio >= 1 && building.Data.CanCollectResources)
            {
                if (!building.Data.CollectableResources.Contains(type))
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

    public string GetHarvestAnimationName()
    {
        if (_resource.Data.Type == ResourceType.Stone || _resource.Data.Type == ResourceType.Gold)
            return "Mine";
        else
            return "Chop";
    }
}
