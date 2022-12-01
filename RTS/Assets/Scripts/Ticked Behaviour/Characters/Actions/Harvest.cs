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

    public override bool Perform()
    {
        if (--_duration < 0f)
        {
            _character.AddAction(new Move(_character, Vector2.zero/*WhereDoIPutMyStuff()*/));
            Vector2Int? newPosition;
            newPosition = (_ressource as Forest)?.GetNextTree(_position);
            newPosition ??= (_ressource as Aggregate)?.Data.Amount > 0 ? _position : null;
            if ((newPosition ??= _position) != _position)//still things to harvest
            {
                Vector2Int harvestingCoords = _ressource.GetHarvestingPosition((Vector2Int)newPosition, _character.Coords);
                _character.AddAction(new Move(_character, (Vector2)harvestingCoords));
                _character.AddAction(new Harvest(_character, _ressource.GetTileToHarvest(harvestingCoords), _ressource));
            }
            return true;
        }
        return false;
    }
}
