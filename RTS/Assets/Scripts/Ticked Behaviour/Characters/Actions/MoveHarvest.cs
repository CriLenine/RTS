using UnityEngine;

public class MoveHarvest : Move
{
    private readonly Vector2? _harvestPosition;
    private readonly IResourceStorer _resourceStorer;
    public MoveHarvest(Character character, Vector2 depositPosition, Vector2? harvestPosition, IResourceStorer resourceStorer)
        : base(character, depositPosition)
    {
        _harvestPosition = harvestPosition;
        _resourceStorer = resourceStorer;
    }

    protected override bool Update()
    {
        if (CharacterManager.Move(_character, Position))
        {
            if (Position == _harvestPosition || _harvestPosition == null)
                ++Index;
            else
            {
                Peon harvester = _character as Peon;
                _resourceStorer.Fill(harvester.CarriedResource);
                harvester.CarriedResource = new Resource.Amount(harvester.CarriedResource.Type);
                Positions[Index] = (Vector2)_harvestPosition;
            }
        }

        return Index == Positions.Count;
    }
}
