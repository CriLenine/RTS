using System.Collections.Generic;
using UnityEngine;

public class MoveHarvest : Move
{
    private readonly Resource _resource;
    private readonly IResourceStorer _resourceStorer;
    public MoveHarvest(Character character, List<Vector2> depositPositions, IResourceStorer resourceStorer, Resource resource) : base(character, depositPositions)
    {
        _resource = resource;
        _resourceStorer = resourceStorer;
    }

    protected override bool Update()
    {
        if (Positions.Count == 0)
            return true;

        if (!CharacterManager.Move(_character, Position))
            return false;

        /* Move to Position was a success */

        if (++Index == Positions.Count)     // Path is completed
        {
            /* The peon arrived to the deposit location */

            Peon harvester = _character as Peon;

            _resourceStorer.Fill(harvester.CarriedResource, _character.Performer);

            harvester.CarriedResource = new Resource.Amount(harvester.CarriedResource.Type);

            Vector2Int? harvestingPosition = _resource.GetHarvestingPosition(_resource.GetFirst(), _character.Coords, _character.Performer);

            if (harvestingPosition != null)
            {
                harvester.SetAction(new Move(_character, LocomotionManager.RetrieveWayPoints(_character.Performer, _character, harvestingPosition.Value)));
                harvester.SetAction(new Harvest(_character, harvestingPosition.Value, _resource));
            }

            return true;
        }

        return false;
    }
}
