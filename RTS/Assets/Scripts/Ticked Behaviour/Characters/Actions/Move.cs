using System;
using UnityEngine;
using System.Collections.Generic;

public class Move : Action
{
    public int Index { get; private set; }

    public readonly Vector2[] Positions;

    public Vector2 Position => Positions[Index];

    public Move(Character character, Vector2[] positions) : base(character)
    {
        Positions = positions;
    }

    public Move(Character character, Vector2 position) : base(character)
    {
        Positions = new Vector2[] { position };
    }

    public override bool Perform()
    {
        if (Index == Positions.Length - 1 && (Position - (Vector2)_character.transform.position).sqrMagnitude < .2f)
            return true;

        if (CharacterManager.Move(_character, Position))
            ++Index;

        return Index == Positions.Length;
    }
}
