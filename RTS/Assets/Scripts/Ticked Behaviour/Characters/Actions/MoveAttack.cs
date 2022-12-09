using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MoveAttack : Move
{

    private float _finalDistanceToDest;

    private TickedBehaviour _target;
    private Vector2Int _lastPos;
    public MoveAttack(Character character, Vector2[] positions, TickedBehaviour target) : base(character, positions)
    {
        _finalDistanceToDest = character.Data.AttackRange;

        _target = target;
        _lastPos = TileMapManager.WorldToTilemapCoords(target.transform.position);
    }

    public MoveAttack(Character character, Vector2 position, TickedBehaviour target) : base(character, position)
    {
        _finalDistanceToDest =character.Data.AttackRange;

        _target = target;
        _lastPos = TileMapManager.WorldToTilemapCoords(target.transform.position);
    }

    protected override bool Update()
    {
        bool output = base.Update();

        if ((Positions[^1] - (Vector2)_character.transform.position).sqrMagnitude <= _finalDistanceToDest)
        {
            return true;
        }


        Vector2Int postToTest = TileMapManager.WorldToTilemapCoords(_target.transform.position);
        if (postToTest != _lastPos)
        {
            if (TileMapManager.LineOfSight(_character.Performer, _character.Coords, postToTest))
            {
                Positions.Clear();
                Positions.Add(postToTest);
            }
            else
                Positions.Add(postToTest);

            _lastPos = postToTest;
        }

        return output;
    }
}
