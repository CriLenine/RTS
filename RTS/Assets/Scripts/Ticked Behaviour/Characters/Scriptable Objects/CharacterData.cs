using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterData : ScriptableObject
{
    [Header("Name")]
    [Space]

    [SerializeField]
    private string _unitName;
    public string UnitName => _unitName;

    [Space]
    [Space]

    [Header("Fighting Stats")]
    [Space]

    [SerializeField]
    [Min(0)]
    private int _maxHealth;
    [SerializeField]
    [Min(0)]
    private int _attackDamage, _meleeArmor, _rangeArmor;
    public int MaxHealth => _maxHealth;
    public int AttackDamage => _attackDamage;
    public int MeleeArmor => _meleeArmor;
    public int RangeArmor => _rangeArmor;

    [SerializeField]
    [Min(0)]
    private float _autoAttackDistance;
    public float AutoAttackDistance => _autoAttackDistance;

    [Space]
    [Space]

    [Header("Weapon")]
    [Space]

    [SerializeField]
    private Sprite _weapon;
    public Sprite Weapon => _weapon;
}
