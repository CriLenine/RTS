using System;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Attack : Action
{
    private TickedBehaviour _target;
    private IDamageable Itarget;
    private int _attackDamage;
    private float _maxAttackDist;

    private UnityEngine.Transform _targetTransform;
    private UnityEngine.Transform _charaTransform;

    private bool _isOrder;
    public Attack(Character character, TickedBehaviour target, bool isOrder = true) : base(character)
    {
        _target = target;

        if(!target.TryGetComponent(out Itarget))
            throw new NotImplementedException("the attack target is not damageable");

        _attackDamage = character.Data.AttackDamage;
        _maxAttackDist = character.Data.AutoAttackDistance;
        _isOrder = isOrder;

        _targetTransform = target.transform;
        _charaTransform = character.transform;
    }

    public override bool Perform()
    {
        if (!_target) return true;//target already dead no point 

        if (((Vector2)_targetTransform.position - (Vector2)_charaTransform.position).sqrMagnitude > _maxAttackDist) // si trop loin on arrete d'attaquer 
        {
            if (_isOrder)//Si on a cliquer a la mano sur lennemie on le suit jusqua la mort
            {
                _character.AddAction(new Move(_character, _target.transform.position, true));
                _character.AddAction(new Attack(_character, _target));
            }

            return true;
        }


        if(Itarget.TakeDamage(_attackDamage)) //sinon tant qu'il n'est pas mort on attaque
        {
            GameManager.DestroyEntity(_target.ID);
            return true;
        }
        return false;
    }
}
