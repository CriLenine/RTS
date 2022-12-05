using UnityEngine;

public class Harvest : Action
{
    private Vector2Int _position;
    private Ressource _ressource;
    private float _duration;
    public Harvest(Character character, Vector2 position, Ressource ressource) : base(character)
    {
        _position = new Vector2Int((int)position.x, (int)position.y);
        _ressource = ressource;
        _duration = ressource.Data.HarvestingTime / 0.025f;
    }

    protected override bool Update()
    {
        if (--_duration < 0f)
        {
            _character.AddAction(new Move(_character, Vector2.zero/*WhereDoIPutMyStuff()*/));
            Vector2Int newPosition = _position;
            Forest forest = _ressource as Forest;
            Aggregate aggregate = _ressource as Aggregate;
            bool continueHarvesting = true;
            if (forest)
            {
                newPosition = forest.GetNextTree(_position);
                continueHarvesting = newPosition != _position;
            }
            else if (aggregate)
            {
                continueHarvesting = --aggregate.Data.Amount > 0;
            }
            if (continueHarvesting)//still things to harvest
            {
                Vector2Int harvestingCoords = _ressource.GetHarvestingPosition(newPosition, _character.Coords);

                AddAction(new Move(_character, TileMapManager.TilemapCoordsToWorld(harvestingCoords)));
                AddAction(new Harvest(_character, _ressource.GetTileToHarvest(harvestingCoords), _ressource));
            }
            Debug.Log("Harvested");
            return true;
        }

        return false;
    }
}
