using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterData : ScriptableObject
{
    [Header("Spawn Data")]
    [Space]

    [SerializeField]
    private Character.Type _type;

    [SerializeField]
    private Character _character;

    [SerializeField]
    [Min(1)]
    private int _spawnTicks;

    [SerializeField]
    private Resource.Amount[] _cost;
    public Resource.Amount[] Cost => _cost;

    [Header("UI")]
    [SerializeField]
    private Sprite _HUDIcon;
    public Character.Type Type => _type;
    public Character Character => _character;
    public Sprite HUDIcon => _HUDIcon;

    public float SpawnTicks => _spawnTicks;

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
    [Min(0.1f)]
    private float _attackRange;
    public float AttackRange => _attackRange;

    [SerializeField]
    [Min(0.1f)]
    private float _attackSpeed;
    public float AttackSpeed => _attackSpeed;

    [Space]
    [Space]

    [Header("Weapon")]
    [Space]

    [SerializeField]
    private Sprite _weapon;
    public Sprite Weapon => _weapon;
}
