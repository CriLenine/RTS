using UnityEngine;
using System.Collections.Generic;

public class Move : Action
{
    public int Index { get; protected set; }

    public readonly List<Vector2> Positions;

    #region Thresholds

    public float TestThreshold = 1f;
    private float defaultThreshold = 0;
    private float thresholdIncrement = .05f;
    public float CompletionThreshold => defaultThreshold += thresholdIncrement;

    #endregion

    public Vector2 Position => Positions[Index];


    public Move(Character character, List<Vector2> positions) : base(character)
    {
        Positions = positions;
    }

    public Move(Character character, Vector2 position) : base(character)
    {
        Positions = new List<Vector2>
        {
            position
        };
    }

    protected override bool Update()
    {
        if (Positions.Count == 0)
            return true;
        if (LocomotionManager.Move(_character, Position))
            ++Index;

        return Index == Positions.Count;
    }
}
