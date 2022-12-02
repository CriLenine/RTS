using System;
using UnityEngine;
using System.Collections.Generic;

public class Move : Action
{
    public int Index { get; private set; }

    public readonly Vector2[] Positions;

    public Vector2 Position => Positions[Index];

    private float _finalDistanceToDest;

    public Move(Character character, Vector2[] positions, bool isAttacking = false) : base(character)
    {
        _finalDistanceToDest = isAttacking ? character.Data.AutoAttackDistance : .2f;
        Positions = positions;
    }

    public Move(Character character, Vector2 position, bool isAttacking=false) : base(character)
    {
        _finalDistanceToDest = isAttacking ? character.Data.AutoAttackDistance: .2f;
        Positions = new Vector2[] { position };
    }



    public override bool Perform()
    {
        if (Index == Positions.Length - 1 && (Position - (Vector2)_character.transform.position).sqrMagnitude < _finalDistanceToDest)
            return true;
     

        if (CharacterManager.Move(_character, Position))
                ++Index;


        return Index == Positions.Length;
    }
}
