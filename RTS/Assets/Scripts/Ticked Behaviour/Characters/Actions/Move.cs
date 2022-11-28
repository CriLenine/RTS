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
        if (CharacterManager.Move(_character, _positions[0]))
            _positions.RemoveAt(0);

        return _positions.Count == 0;
    }

    public Vector2 currentWayPoint()
    {
        return _positions[0];
    }
}
