using MyBox;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "CharacterData", order = 1)]

public class CharacterData : TickedBehaviorData
{
    [SerializeField]
    private List<AudioClip> _genericOrderAudios;
    [SerializeField]
    private List<AudioClip> _attackOrderAudios;

    public List<AudioClip> GenericOrderAudios => _genericOrderAudios;
    public List<AudioClip> AttackOrderAudios => _attackOrderAudios;

    [Separator("Spawn Data")]
    [Space]

    [SerializeField]
    private Character.Type _type;
    public Character.Type Type => _type;

    [SerializeField]
    private SubType _subType;
    public SubType SubType => _subType;

    [SerializeField, Min(1)]
    private int _spawnTicks;
    public float SpawnTicks => _spawnTicks;

    [SerializeField]
    private Resource.Amount[] _cost;
    public Resource.Amount[] Cost => _cost;

    [Separator("UI")]
    [Space]

    [SerializeField]
    private Sprite _icon;
    public Sprite Icon => _icon;

    [SerializeField]
    private Color _color;
    public Color Color => _color;

    [Separator("ToolTip")]
    [Space]

    [SerializeField]
    private ToolTip _toolTip;
    public ToolTip ToolTip => _toolTip;

    [SerializeField]
    private ToolTip _selectionToolTip;
    public ToolTip SelectionToolTip => _selectionToolTip;

    [Separator("Art")]
    [Space]

    [SerializeField]
    private RuntimeAnimatorController _animator;
    public RuntimeAnimatorController AnimatorCtrller => _animator;

    [SerializeField]
    private Sprite _characterSprite;
    public Sprite CharacterSprite => _characterSprite;

    [Separator("Game Specs")]
    [Space]

    [SerializeField, Min(0)]
    private int _maxHealth;
    public int MaxHealth => _maxHealth;

    [Space]

    [SerializeField, Min(0)]
    private int _attackDamage;
    public int AttackDamage => _attackDamage;

    [SerializeField, Min(0.1f)]
    private float _attackRange;
    public float AttackRange => _attackRange;

    [SerializeField, Min(0.1f)]
    private float _attackSpeed;
    public float AttackSpeed => _attackSpeed;

    [Space]

    [SerializeField, Min(0.1f)]
    private int _meleeArmor;
    public int MeleeArmor => _meleeArmor;

    [SerializeField, Min(0)]
    private int _rangeArmor;
    public int RangeArmor => _rangeArmor;

    [Space]

    [SerializeField]
    private Sprite _weapon;
    public Sprite Weapon => _weapon;

    [Separator("Economy Stats")]
    [Space]

    [SerializeField]
    private bool _canHarvestResources;
    public bool CanHarvestResources => _canHarvestResources;

    [SerializeField, ConditionalField(nameof(_canHarvestResources))]

    private int _maxCarriedResources, _amountGetPerHarvest, _harvestingSpeed;
    public int MaxCarriedResources => _maxCarriedResources;
    public int AmountGetPerHarvest => _amountGetPerHarvest;
    public int HarvestingSpeed => _harvestingSpeed;

    [Space]

    [SerializeField]
    private bool _canBuild;
    public bool CanBuild => _canBuild;

    [SerializeField, ConditionalField(nameof(_canBuild))]
    private int _buildEfficiencyMultiplier;
    public int BuildEfficiencyMultiplier => _buildEfficiencyMultiplier;
}
