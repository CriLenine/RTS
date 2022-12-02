using System;
using UnityEngine;
using System.Collections.Generic;

public class Move : Action
{
    public int Index { get; private set; }

    public readonly Vector2[] Positions;

    #region Thresholds

    public float TestThreshold = 1f;
    private float defaultThreshold = 0;
    private float thresholdIncrement = .05f;
    public float CompletionThreshold => defaultThreshold += thresholdIncrement;

    #endregion

    public Vector2 Position => Positions[Index];
    private bool _movingToFinalWayPoint => Index == Positions.Length - 1;

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
        if (CharacterManager.Move(_character, Position, _movingToFinalWayPoint))
            ++Index;

        return Index == Positions.Length;
    }
}
