using System.Collections.Generic;
using UnityEngine;

public class Harvest : Action
{
    private Vector2Int _coords;
    private Resource _resource;
    private float _duration;
    public Harvest(Character character, Vector2 unroundedCoords, Resource resource) : base(character)
    {
        _coords = new Vector2Int((int)unroundedCoords.x, (int)unroundedCoords.y);
        _resource = resource;
        _duration = resource.Data.HarvestingTime / 0.025f;
    }

    protected override bool Update()
    {
        if (--_duration < 0f)
        {
            Vector2Int newCoords = _coords;
            bool continueHarvesting = false;
            if (_resource is Forest forest)
            {
                newCoords = forest.GetNextTree(_coords);
                continueHarvesting = newCoords != _coords;
            }
            else if (_resource is Aggregate aggregate)
            {
                continueHarvesting = --aggregate.Data.Amount > 0;
            }

            Building building = GetNearestResourceStorer(_resource.Data.Type);
            Vector2? harvestingPosition = continueHarvesting ? 
                TileMapManager.TilemapCoordsToWorld(_resource.GetHarvestingPosition(newCoords, _character.Coords)) : null;
            AddAction(new MoveHarvest(_character, building.transform.position, harvestingPosition, (IResourceStorer)building));
            AddAction(new Harvest(_character, newCoords, _resource));
            ((Peon)_character).CarriedResource = (_resource.Data.Type, _resource.Data.AmountPerHarvest);
            Debug.Log("Harvested");
            return true;
        }

        return !GameManager.resourcesManager.Harvestable(_coords);
    }

    private Building GetNearestResourceStorer(ResourceType type)
    {
        Building closestBuilding = null;
        float shortestDistance = float.MaxValue;
        foreach (Building building in GameManager.MyBuildings)
        {
            if (building is IResourceStorer resourceStorer)
            {
                if (resourceStorer.StorableResources.Contains(type))
                {
                    closestBuilding ??= building;
                    float distance = (building.transform.position - _character.transform.position).sqrMagnitude;
                    if (distance < shortestDistance)
                    {
                        closestBuilding = building;
                        shortestDistance = distance;
                    }
                }
            }
        }
        return closestBuilding;
    }
}
