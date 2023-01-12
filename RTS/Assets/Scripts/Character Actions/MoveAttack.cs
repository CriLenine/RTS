using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MoveAttack : Move
{

    private float _finalDistanceToDest;

    private TickedBehaviour _target;
    private Vector2Int _lastPos;
    public MoveAttack(Character character, List<Vector2> positions, TickedBehaviour target) : base(character, positions)
    {
        _finalDistanceToDest = character.Data.AttackRange;

        _target = target;
        _lastPos = _target.Coords;
    }

    public MoveAttack(Character character, Vector2 position, TickedBehaviour target) : base(character, position) // Move and attack target
    {
        _finalDistanceToDest =character.Data.AttackRange;

        _target = target;
        _lastPos = _target.Coords;
    }

    protected override bool Update()
    {
        bool output = base.Update();

        if ((Positions[^1] - (Vector2)_character.transform.position).sqrMagnitude <= _finalDistanceToDest || _target == null)
        {
            return true;
        }


        Vector2Int postToTest =_target.Coords;
        if (postToTest != _lastPos)
        {
            if (TileMapManager.LineOfSight(_character.Performer, _character.Coords, postToTest))
            {
                Positions.Clear();
                Index = 0;
                Positions.Add(TileMapManager.TilemapCoordsToWorld(postToTest));
            }
            else
                Positions.Add(postToTest);

            _lastPos = postToTest;
        }

        return output;
    }
}
