using System.Collections.Generic;
using UnityEngine;

public class Harvest : Action
{
    private Vector2Int _coords;
    private Ressource _ressource;
    private float _duration;
    public Harvest(Character character, Vector2 unroundedCoords, Ressource ressource) : base(character)
    {
        _coords = new Vector2Int((int)unroundedCoords.x, (int)unroundedCoords.y);
        _ressource = ressource;
        _duration = ressource.Data.HarvestingTime / 0.025f;
    }

    protected override bool Update()
    {
        if (--_duration < 0f)
        {
            Vector2Int newCoords = _coords;
            bool continueHarvesting = false;
            if (_ressource is Forest forest)
            {
                newCoords = forest.GetNextTree(_coords);
                continueHarvesting = newCoords != _coords;
            }
            else if (_ressource is Aggregate aggregate)
            {
                continueHarvesting = --aggregate.Data.Amount > 0;
            }

            Building building = GetNearestRessourceStorer();
            Vector2? harvestingPosition = continueHarvesting ? 
                TileMapManager.TilemapCoordsToWorld(_ressource.GetHarvestingPosition(newCoords, _character.Coords)) : null;
            AddAction(new MoveHarvest(_character, building.transform.position, harvestingPosition, (IRessourceStorer)building));
            AddAction(new Harvest(_character, newCoords, _ressource));
            ((Peon)_character).CarriedRessource = (_ressource.Data.Type, _ressource.Data.AmountPerHarvest);
            Debug.Log("Harvested");
            return true;
        }

        return !GameManager.RessourcesManager.Harvestable(_coords);
    }

    private Building GetNearestRessourceStorer()
    {
        Building closestBuilding = null;
        float shortestDistance = float.MaxValue;
        foreach (Building building in GameManager.MyBuildings)
        {
            if (building is IRessourceStorer)
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
        return closestBuilding;
    }
}
