using System;
using Unity.VisualScripting;
using UnityEngine;

public class Attack : Action
{
    private TickedBehaviour _target;
    private IDamageable Itarget;
    private int _attackDamage;
    private int _maxAttackDist;

    private bool _isOrder;
    public Attack(Character character, TickedBehaviour target, bool isOrder) : base(character)
    {
        _target = target;

        if(!target.TryGetComponent(out Itarget))
            throw new NotImplementedException("the attack target is not damageable");

        _attackDamage = character.Data.AttackDamage;
        _maxAttackDist = character.Data.AutoAttackDistance;
        _isOrder = isOrder;
    }

    public override bool Perform()
    {
        float dist = Vector2.Distance(_character.transform.position, _target.transform.position);

        if (dist > _maxAttackDist) // si trop loin on arrete d'attaquer 
        {
            if (_isOrder)//Si on a cliquer a la mano sur lennemie on le suit jusqua la mort
                _character.AddAction(new Move(_character, _target.transform.position));

            return false;
        }


        if(Itarget.TakeDamage(_attackDamage)) //sinon tant qu'il n'est pas mort on attaque
        {
            GameManager.DestroyEntity(_target.ID);
            return true;
        }
        return false;
    }
}
