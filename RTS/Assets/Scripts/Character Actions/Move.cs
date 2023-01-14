using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Move : Action
{
    public int Index { get; protected set; }

    public readonly List<Vector2> Positions;

    #region Thresholds

    public float TestThreshold = 1f;
    private float defaultThreshold = 0;
    private float thresholdIncrement = .05f;
    public float CompletionThreshold => defaultThreshold += thresholdIncrement;

    public Vector2 LastDir;
    public HashSet<float> TestedAngles = new HashSet<float>();
    public float? LastAngle = null;

    #endregion

    public Vector2 Position => Positions[Index];

    public Vector2 CharacterPosition => (Vector2)_character.transform.position;
    private Animator _animator;

    public Vector2 Direction => Position - CharacterPosition;

    public Move(Character character, List<Vector2> positions) : base(character)
    {
        Positions = positions;

        LastDir = Direction.normalized;
        _animator = character.Animator;
    }

    public Move(Character character, Vector2 position) : base(character)
    {
        Positions = new List<Vector2> { position };

        LastDir = Direction.normalized;
        _animator = character.Animator;
    }

    protected override bool Update()
    {
        if (Positions.Count == 0)
            return true;

        _character.Animator.Play("Walk");

        Vector2 lastpos = _character.Position;

        if (LocomotionManager.Move(_character, Position))
        {
            ++Index;
        }

        var diff = _character.Position - lastpos;

        _animator.SetFloat("MoveX", diff.x);
        _animator.SetFloat("MoveY", diff.y);

        return Index == Positions.Count;
    }
}
