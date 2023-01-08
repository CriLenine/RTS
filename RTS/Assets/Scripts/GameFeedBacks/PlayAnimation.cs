using RTS.Feedback;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[GameFeedback(51, 70, 30, "PlayAnimation")]
public class PlayAnimation : GameFeedback
{
    public bool isCharaAnimation = true;
    [DrawIf("isCharaAnimation",true)]
    [SerializeField] private string _charaAnimationName;
    [DrawIf("isCharaAnimation", false)]
    [SerializeField] private Animator _animator;
    [DrawIf("isCharaAnimation", false)]
    [SerializeField] private string _animationName;
    protected override void Execute(GameObject gameObject)
    {
        if(!isCharaAnimation)
        {
            _animator.Play(_animationName);
            return;
        }

        if (!gameObject.TryGetComponent(out Character character)) return;

        if (character.Data.Type != Character.Type.Bowman) return;

        character.Animator.Play(_charaAnimationName);
    }

    public override string ToString()
    {
        return $"Play {(isCharaAnimation ? _charaAnimationName : _animationName)}";
    }
}
