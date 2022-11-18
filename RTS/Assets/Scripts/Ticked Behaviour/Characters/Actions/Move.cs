using UnityEngine;

public class Move : Action
{
    private Vector2 _position;

    public Move(Character character, Vector2 position) : base(character)
    {
        _position = position;
    }

    public override bool Perform()
    {
        return CharacterManager.Move(_character, _position);
    }
}
