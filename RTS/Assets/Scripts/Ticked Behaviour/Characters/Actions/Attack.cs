using System;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Attack : Action
{
    private TickedBehaviour _target;
    private IDamageable Itarget;
    private int _attackDamage;
    private float _attackRange;
    private float _attackSpeed;

    private Vector2 _posToAttack;
    private UnityEngine.Transform _charaTransform;

    private bool _isOrder;

    private float _attackSpeedTimer = 0;
    public Attack(Character character, TickedBehaviour target, bool isOrder = true) : base(character)
    {
        _target = target;

        if (!target.TryGetComponent(out Itarget))
            throw new NotImplementedException("the attack target is not damageable");

        _attackDamage = character.Data.AttackDamage;
        _attackRange = character.Data.AttackRange;
        _attackSpeed = character.Data.AttackSpeed;
        _isOrder = isOrder;


        _posToAttack = target.transform.position;
        _charaTransform = character.transform;
    }
    public Attack(Character character, TickedBehaviour target,Vector2 posToAttack, bool isOrder = true) : base(character)
    {
        _target = target;

        if(!target.TryGetComponent(out Itarget))
            throw new NotImplementedException("the attack target is not damageable");

        _attackDamage = character.Data.AttackDamage;
        _attackRange = character.Data.AttackRange;
        _attackSpeed = character.Data.AttackSpeed;
        _isOrder = isOrder;

        
        _posToAttack = posToAttack;
        _charaTransform = character.transform;
    }

    protected override bool Update()
    {
        if (!_target) return true;//target already dead no point 

        if ((_posToAttack - (Vector2)_charaTransform.position).sqrMagnitude > _attackRange) // si trop loin on arrete d'attaquer 
        {
            if (_isOrder)//Si on a cliquer a la mano sur lennemie on le suit jusqua la mort
            {
                SetAction(new MoveAttack(_character, _target.transform.position, _target));
            }

            return true;
        }

        if(_attackSpeedTimer == 0)
            if (Itarget.TakeDamage(_attackDamage)) //sinon tant qu'il n'est pas mort on attaque
            {
                if (_target is Building && _target.Performer == NetworkManager.Me) 
                {
                    Building building = (Building)_target;
                    GameManager.UpdateHousing(-building.Data.HousingProvided);
                }
                GameManager.DestroyEntity(_target.ID);
                return true;
            }

        var newTime = _attackSpeedTimer + NetworkManager.NormalTickPeriod;
        _attackSpeedTimer = newTime  >= _attackSpeed ? 0 : newTime;

        return false;
    }
}
