using System.Collections.Generic;
using UnityEngine;

public class MoveHarvest : Move
{
    private readonly Vector2Int? _harvestingCoords;
    private readonly IResourceStorer _resourceStorer;
    private readonly int _performer;
    public MoveHarvest(Character character, Vector2 depositPosition, Vector2Int harvestPosition, IResourceStorer resourceStorer, int performer) : base(character, depositPosition)
    {
        _harvestingCoords =  harvestPosition;
        _resourceStorer = resourceStorer;
        _performer = performer;
    }

    public MoveHarvest(Character character, List<Vector2> depositPositions, Vector2Int? harvestPosition, IResourceStorer resourceStorer, int performer) : base(character, depositPositions)
    {
        _harvestingCoords = harvestPosition;
        _resourceStorer = resourceStorer;
        _performer = performer;
    }

    protected override bool Update()
    {
        if (Positions.Count == 0)
            return true;
        if (CharacterManager.Move(_character, Position))
        {
            if (++Index == Positions.Count)
            {
                if (Positions[^1] == _harvestingCoords || _harvestingCoords == null)
                    return true;
                Peon harvester = _character as Peon;
                _resourceStorer.Fill(harvester.CarriedResource);
                harvester.CarriedResource = new Resource.Amount(harvester.CarriedResource.Type);
                Positions.Clear();
                Positions.AddRange(LocomotionManager.RetrieveWayPoints(_performer, _character, (Vector2Int)_harvestingCoords));
                Index = 0;
            }
        }

        return false;
    }
}
