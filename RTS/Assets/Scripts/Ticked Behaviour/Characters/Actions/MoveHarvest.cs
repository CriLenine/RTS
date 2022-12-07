using UnityEngine;

public class MoveHarvest : Move
{
    private readonly Vector2? _harvestPosition;
    private readonly IRessourceStorer _ressourceStorer;
    public MoveHarvest(Character character, Vector2 depositPosition, Vector2? harvestPosition, IRessourceStorer ressourceStorer)
        : base(character, depositPosition)
    {
        _harvestPosition = harvestPosition;
        _ressourceStorer = ressourceStorer;
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
                (RessourceType type, int amount) = harvester.CarriedRessource;
                _ressourceStorer.Fill(type, amount);
                harvester.CarriedRessource = (type, 0);
                Positions[Index] = (Vector2)_harvestPosition;
            }
        }

        return Index == Positions.Count;
    }
}
