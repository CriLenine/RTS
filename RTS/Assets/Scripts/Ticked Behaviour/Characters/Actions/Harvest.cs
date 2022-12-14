using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Harvest : Action
{
    private readonly Vector2Int _coords;
    private readonly Vector2Int _attractionPoint;
    private readonly Resource _resource;
    private float _duration;
    private readonly int _performer;
    public Harvest(Character character, Vector2Int coords, Vector2Int attractionPoint, Resource resource, int performer) : base(character)
    {
        _coords = coords;
        _attractionPoint = attractionPoint;
        _resource = resource;
        _duration = resource.Data.HarvestingTime / 0.025f;
        _performer = performer;
    }

    protected override bool Update()
    {
        if (--_duration < 0f || !ResourcesManager.Harvestable(_coords))
        {
            Peon peon = _character as Peon;

            if (_duration < 0f)
            {
                if (peon.CarriedResource.Value == 0 || peon.CarriedResource.Type != _resource.Data.Type) //if peon carries other resource or nothing
                {
                    peon.CarriedResource = new Resource.Amount(_resource.Data.Type);
                }

                peon.CarriedResource = peon.CarriedResource.AddQuantity(_resource.Data.AmountPerHarvest);
            }

            Stopwatch sw = Stopwatch.StartNew();
            Vector2Int? newInputCoords = _resource.GetNext(_coords, _attractionPoint, _character.Coords, _performer, _duration < 0f);
            sw.Stop();
            //Debug.Log($"GetNext in {sw.Elapsed.TotalMilliseconds} ms");
            if (newInputCoords == null)
            {
                Debug.Log("No available tile found to continue harvest.");
                return true;
            }
            sw = Stopwatch.StartNew();
            Vector2Int nextCoordsToGo = _resource.GetHarvestingPosition((Vector2Int)newInputCoords, _character.Coords, _performer);
            sw.Stop();
            //Debug.Log($"GetHarvestingPosition in {sw.Elapsed.TotalMilliseconds} ms");

            Vector2Int nextCoordsToHarvest = _resource.GetTileToHarvest(nextCoordsToGo, _attractionPoint);

            Action nextAction;

            List<Vector2> wayPointsToGo = LocomotionManager.RetrieveWayPoints(_performer, _character, nextCoordsToGo);

            if(wayPointsToGo == null)
                Debug.Log("fdp go");

            if (peon.CarriedResource.Value >= peon.Data.NMaxCarriedResources) //need to deposit
            {
                Building building = GetNearestResourceStorer(_resource.Data.Type);
                if (building == null)
                {
                    Debug.Log("No suitable resource storer found");
                    return true;
                }
                List<Vector2> wayPointsToDeposit = LocomotionManager.RetrieveWayPoints(_performer, _character, GetDepositPosition(building));

                if (wayPointsToDeposit == null)
                    Debug.Log("fdp deposit");
                nextAction = new MoveHarvest(_character, wayPointsToDeposit, nextCoordsToGo, (IResourceStorer)building, _performer);
            }
            else
                nextAction = new Move(_character, wayPointsToGo);

            AddAction(nextAction);
            AddAction(new Harvest(_character, nextCoordsToHarvest, _attractionPoint, _resource, _performer));

            return true;
        }

        return false;
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
                    if (TileMapManager.GetLogicalTile(tileCoords)?.IsFree(_performer) == true
                        && TileMapManager.FindPath(_performer, _character.Coords, tileCoords)?.Count > 0)
                        availableTiles.Add(tileCoords);
                }
            }

            //If we found at least one candidate
            if (availableTiles.Count > 0)
                return Resource.FindClosestCoords(availableTiles, _character.Coords);
        }

        Debug.LogError("Cannot deposit to the building");
        return Vector2Int.zero;
    }
}
