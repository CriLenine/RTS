using System;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Attack : Action
{
    private TickedBehaviour _target;
    private IDamageable Itarget;

    //Attackers data
    private int _attackDamage;
    private float _attackRange;
    private float _attackSpeed;

    //Variable
    private float _attackSpeedTimer = 0;
    private bool _isbuilding = false;
    private bool _isOrder;
    public Attack(Character character, TickedBehaviour target,bool targetIsBuilding, bool isOrder = true) : base(character)
    {
        _target = target;

        if (!target.TryGetComponent(out Itarget))
            throw new NotImplementedException("the attack target is not damageable");

        _attackDamage = character.Data.AttackDamage;
        _attackRange = character.Data.AttackRange;
        _attackSpeed = character.Data.AttackSpeed;
        _isOrder = isOrder;

        _isbuilding = targetIsBuilding;
        character.SetTarget(target);
    }

    protected override bool Update()
    {
        if (_target == null) return true;//target already dead no point 

        if (_target is Character character && character.CurrentHealth == 0)
            return true;

        if (_target is Building building && building.CurrentHealth == 0)
            return true;

        if (_isbuilding && (_target.Position - _character.Position).sqrMagnitude > _attackRange) // si trop loin on arrete d'attaquer 
        {
            if (_isOrder)//Si on a cliquer a la mano sur lennemie on le suit jusqua la mort
            {
                SetAction(new MoveAttack(_character, _target.transform.position, _target));
                AddAction(new Attack(_character, _target, _isbuilding));
            }

            return true;
        }

        if (_attackSpeedTimer == 0)
        {
            if (_character.Data.Type is Character.Type.Bowman)
                GameEventsManager.PlayEvent("ShootArrow",_character.gameObject);
            else
                GameEventsManager.PlayEvent("AttackSword", _character.gameObject);

            if (Itarget.TakeDamage(_attackDamage)) //sinon tant qu'il n'est pas mort on attaque
            {
                if (_target is Building build)
                {
                    StatsManager.IncreaseBuildingsDestroyed(_character.Performer);
                    StatsManager.IncreaseBuildingsLost(build.Performer);

                    if (_target.Performer == NetworkManager.Me)
                        GameManager.UpdateHousing(-build.Data.HousingProvided);
                }

                if (_target is Character chara)
                {
                    StatsManager.IncreaseUnitsKilled(_character.Performer);
                    StatsManager.IncreaseUnitsLost(chara.Performer);
                    chara.Animator.Play("Die");
                }

                GameManager.DestroyEntity(_target.ID);
                return true;
            }
        }


        var newTime = _attackSpeedTimer + NetworkManager.NormalTickPeriod;
        _attackSpeedTimer = newTime >= _attackSpeed ? 0 : newTime;

        return false;
    }
}
