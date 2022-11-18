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
        /* LocomotionManager.Move(character); */
        throw new System.NotImplementedException();
    }
}
