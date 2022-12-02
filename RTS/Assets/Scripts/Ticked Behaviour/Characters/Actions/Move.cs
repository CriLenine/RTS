using System;
using UnityEngine;
using System.Collections.Generic;

public class Move : Action
{
    public int Index { get; private set; }

    public readonly List<Vector2> Positions;

    public Vector2 Position => Positions[Index];

    private float _finalDistanceToDest;

    private TickedBehaviour _target;
    private Vector2Int _lastPos;
    private bool _isAttacking;
    public Move(Character character, Vector2[] positions, bool isAttacking = false) : base(character)
    {
        _finalDistanceToDest = isAttacking ? character.Data.AutoAttackDistance : .2f;

        Positions = new List<Vector2>(positions);
    }
    public Move(Character character, Vector2[] positions, TickedBehaviour target , bool isAttacking = true) : base(character)
    {
        _finalDistanceToDest = isAttacking ? character.Data.AutoAttackDistance : .2f;
        Positions = new List<Vector2>(positions);


        _target= target;
        _lastPos =TileMapManager.WorldToTilemapCoords( target.transform.position);
        _isAttacking = isAttacking;
    }

    public Move(Character character, Vector2 position, bool isAttacking=false) : base(character)
    {
        _finalDistanceToDest = isAttacking ? character.Data.AutoAttackDistance: .2f;
        Positions = Positions = new List<Vector2>();
        Positions.Add(position);
    }



    public override bool Perform()
    {
        //Index == Positions.Count - 1 &&
     
        if(_isAttacking)
        {
            Vector2Int postToTest = TileMapManager.WorldToTilemapCoords(_target.transform.position);
            if (postToTest != _lastPos)
            {
                if(TileMapManager.LineOfSight(_character.Coords, postToTest))
                {
                    Positions.Clear();
                    Positions.Add(postToTest);
                }    
                else
                    Positions.Add(postToTest);

                _lastPos = postToTest;
            }
        }

        if (CharacterManager.Move(_character, Position))
                ++Index;

        if ((Position - (Vector2)_character.transform.position).sqrMagnitude <= _finalDistanceToDest)
            return true;

        return Index == Positions.Count;
    }
}
