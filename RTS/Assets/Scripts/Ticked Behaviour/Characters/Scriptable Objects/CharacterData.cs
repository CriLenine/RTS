using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterData : ScriptableObject
{
    [SerializeField]
    [Min(0)]
    private int _maxHealth;
    public int MaxHealth => _maxHealth;

    [SerializeField]
    [Min(0)]
    private int _attackDamage;
    public int AttackDamage => _attackDamage;

    [SerializeField]
    [Min(0)]
    private float _autoAttackDistance;
    public float AutoAttackDistance => _autoAttackDistance;

    [SerializeField]
    private UtilsView[] _views;
    public UtilsView[] Views => _views;
}
