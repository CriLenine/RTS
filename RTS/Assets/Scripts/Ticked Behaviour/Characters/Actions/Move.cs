using UnityEngine;
using System.Collections.Generic;

public class Move : Action
{
    public int Index { get; private set; }

    public readonly List<Vector2> Positions;

    #region Thresholds

    public float TestThreshold = 1f;
    private float defaultThreshold = 0;
    private float thresholdIncrement = .05f;
    public float CompletionThreshold => defaultThreshold += thresholdIncrement;

    #endregion

    public Vector2 Position => Positions[Index];


    public Move(Character character, Vector2[] positions) : base(character)
    {
        Positions = new List<Vector2>(positions);

    }

    public Move(Character character, Vector2 position) : base(character)
    {
        Positions = Positions = new List<Vector2>();
        Positions.Add(position);
    }

    protected override bool Update()
    {
        if (CharacterManager.Move(_character, Position))
            ++Index;

        return Index == Positions.Count;
    }
}
