using System.Collections.Generic;
using UnityEngine;

public class MoveHarvest : Move
{
    private readonly Resource _resource;
    private readonly IResourceStorer _resourceStorer;
    private readonly int _performer;
    public MoveHarvest(Character character, List<Vector2> depositPositions, IResourceStorer resourceStorer, Resource resource, int performer) : base(character, depositPositions)
    {
        _resource = resource;
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

        if (++Index == Positions.Count)     // Path is completed
        {
            /* The peon arrived to the deposit location */

            Peon harvester = _character as Peon;

            _resourceStorer.Fill(harvester.CarriedResource, _performer);

            harvester.CarriedResource = new Resource.Amount(harvester.CarriedResource.Type);

            return true;
        }

        return false;
    }
}
