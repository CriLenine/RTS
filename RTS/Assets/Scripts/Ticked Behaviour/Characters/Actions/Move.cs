using UnityEngine;
using System.Collections.Generic;

public class Move : Action
{
    private List<Vector2> _positions = new List<Vector2>();

    public Move(Character character, List<Vector2> positions) : base(character)
    {
        _positions = positions;
    }

    public override bool Perform()
    {
        if(_positions.Count == 1 && (_positions[0] - (Vector2)_character.transform.position).sqrMagnitude < .2f)
            return true;

        if (CharacterManager.Move(_character, _positions[0]))
            _positions.RemoveAt(0);

        return _positions.Count == 0;
    }

    public Vector2 CurrentWayPoint()
    {
        return _positions[0];
    }

    public Vector2 FinalWayPoint()
    {
        return _positions[^1];
    }
}
