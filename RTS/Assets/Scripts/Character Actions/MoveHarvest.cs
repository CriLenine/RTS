using System.Collections.Generic;
using UnityEngine;

public class MoveHarvest : Move
{
    private readonly Resource _resource;
    private readonly Building _resourceStorer;
    private readonly Vector2Int _initialCoords;
    public MoveHarvest(Character character, List<Vector2> depositPositions, Building resourceStorer, Resource resource) : base(character, depositPositions)
    {
        _resource = resource;
        _resourceStorer = resourceStorer;
        _initialCoords = character.Coords;
    }

    protected override bool Update()
    {
        if (Positions.Count == 0)
            return true;

        if (!LocomotionManager.Move(_character, Position))
            return false;

        /* Move to Position was a success */

        if (++Index == Positions.Count)     // Path is completed
        {
            /* The peon arrived to the deposit location */

            Character harvester = _character;

            _resourceStorer.Fill(harvester.HarvestedResource, _character.Performer);

            harvester.SetResource(new Resource.Amount(harvester.HarvestedResource.Type));

            Vector2Int newInputCoords = _resource.GetNext(_character.Coords, _initialCoords,  _character.Performer) ?? _resource.GetClosest(_initialCoords);

            Vector2Int? harvestingPosition = _resource.GetHarvestingPosition(newInputCoords, _character.Coords, _character.Performer);

            if (harvestingPosition != null)
            {
                harvester.SetAction(new Move(_character, LocomotionManager.RetrieveWayPoints(_character.Performer, _character, harvestingPosition.Value)));
                harvester.AddAction(new Harvest(_character, harvestingPosition.Value, _resource));
            }

            return true;
        }

        return false;
    }
}
