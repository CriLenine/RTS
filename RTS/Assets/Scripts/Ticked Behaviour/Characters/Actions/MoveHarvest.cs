using System.Collections.Generic;
using UnityEngine;

public class MoveHarvest : Move
{
    private readonly Vector2Int _harvestingCoords;
    private readonly IResourceStorer _resourceStorer;
    private readonly int _performer;
    public MoveHarvest(Character character, Vector2 depositPosition, Vector2Int harvestPosition, IResourceStorer resourceStorer, int performer) : base(character, depositPosition)
    {
        _harvestingCoords =  harvestPosition;
        _resourceStorer = resourceStorer;
        _performer = performer;
    }

    public MoveHarvest(Character character, List<Vector2> depositPositions, Vector2Int harvestPosition, IResourceStorer resourceStorer, int performer) : base(character, depositPositions)
    {
        _harvestingCoords = harvestPosition;
        _resourceStorer = resourceStorer;
        _performer = performer;
    }

    protected override bool Update()
    {
        if (Positions.Count == 0)
            return true;

        if (!CharacterManager.Move(_character, Position))
            return false;

        /* Move to Position was a success */

        if (++Index == Positions.Count)     // A part of the path is completed
        {
            if (Positions[^1] == _harvestingCoords)    // All the path is completed
                return true;

            /* The peon arrived to the deposit location */

            Peon harvester = _character as Peon;

            _resourceStorer.Fill(harvester.CarriedResource, _performer);

            harvester.CarriedResource = new Resource.Amount(harvester.CarriedResource.Type);

            Positions.Clear();
            List<Vector2> path = LocomotionManager.RetrieveWayPoints(_performer, _character, _harvestingCoords);
            if (path == null)
            {
                Debug.Log("Pathfinding failed.");
                return true;
            }
            Positions.AddRange(path);

            Index = 0;
        }

        return false;
    }
}
