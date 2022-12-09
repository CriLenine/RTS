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
            Peon peon = _character as Peon;

            if (peon.CarriedResource.Value == 0 || peon.CarriedResource.Type != _resource.Data.Type) //if peon carries other resource or nothing
            {
                peon.CarriedResource = new Resource.Amount(_resource.Data.Type);
            }

            peon.CarriedResource = peon.CarriedResource.AddQuantity(_resource.Data.AmountPerHarvest);

            Vector2Int? newInputCoords = _resource.GetNext(_coords);
            if (newInputCoords == null)
            {
                Debug.Log("No available tile found to continue harvest.");
                return true;
            }

            Vector2Int nextCoordsToGo = _resource.GetHarvestingPosition((Vector2Int)newInputCoords, _character.Coords);

            Vector2 harvestingPosition = TileMapManager.TilemapCoordsToWorld(nextCoordsToGo);
            Vector2Int nextCoordsToHarvest = _resource.GetTileToHarvest(nextCoordsToGo);

            Action nextAction;
            if (peon.CarriedResource.Value >= peon.Data.NMaxCarriedResources) //need to deposit
            {
                Building building = GetNearestResourceStorer(_resource.Data.Type);
                if (building == null)
                {
                    Debug.Log("No suitable resource storer found");
                    return true;
                }

                nextAction = new MoveHarvest(_character, building.transform.position, harvestingPosition, (IResourceStorer)building);
            }
            else
                nextAction = new Move(_character, harvestingPosition);

            AddAction(nextAction);
            AddAction(new Harvest(_character, nextCoordsToHarvest, _resource));

            return true;
        }

        return !GameManager.ResourcesManager.Harvestable(_coords);
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
