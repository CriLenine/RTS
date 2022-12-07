using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterData : ScriptableObject
{
    [SerializeField]
    [Min(1)]
    private int _maxHealth;
    public int MaxHealth => _maxHealth;

    [SerializeField]
    [Min(0.1f)]
    private int _attackDamage;
    public int AttackDamage => _attackDamage;

    [SerializeField]
    [Min(0.1f)]
    private float _attackRange;
    public float AttackRange => _attackRange;

    [SerializeField]
    [Min(0.1f)]
    private float _attackSpeed;
    public float AttackSpeed => _attackSpeed;

    [SerializeField]
    private UtilsView[] _views;
    public UtilsView[] Views => _views;
}
