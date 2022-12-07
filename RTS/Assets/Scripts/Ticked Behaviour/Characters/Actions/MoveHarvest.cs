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
                (ResourceType type, int amount) = harvester.CarriedResource;
                _resourceStorer.Fill(type, amount);
                harvester.CarriedResource = (type, 0);
                Positions[Index] = (Vector2)_harvestPosition;
            }
        }

        return Index == Positions.Count;
    }
}
