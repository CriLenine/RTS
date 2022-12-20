﻿using System.Collections.Generic;
using UnityEngine;

public class MoveHarvest : Move
{
    private readonly Resource _resource;
    private readonly Building _resourceStorer;
    public MoveHarvest(Character character, List<Vector2> depositPositions, Building resourceStorer, Resource resource) : base(character, depositPositions)
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

            Character harvester = _character;

            _resourceStorer.Fill(harvester.HarvestedResource, _character.Performer);

            harvester.SetResource(new Resource.Amount(harvester.HarvestedResource.Type));

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
