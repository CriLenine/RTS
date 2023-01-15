using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

[System.Serializable]
public class TNeedCharacter : ActionNode
{
    [SerializeField]
    private Character.Type _characterType;

    [SerializeField]
    private int _minCharacterCount;

    protected override State OnUpdate()
    {
        return context.CharacterCount[_characterType] < _minCharacterCount ? State.Success : State.Failure;
    }
}
