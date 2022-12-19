using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "CharacterData", menuName = "CharacterData", order = 1)]

public class CharacterData : ButtonData
{
    [Space]
    [Separator("Spawn Data")]

    [SerializeField]
    private Character.Type _type;
    public Character.Type Type => _type;

    [SerializeField]
    private SubType _subType;
    public SubType SubType => _subType;

    [SerializeField]
    private Character _prefab;
    public Character Prefab => _prefab;

    [SerializeField]
    [Min(1)]
    private int _spawnTicks;
    public float SpawnTicks => _spawnTicks;

    [SerializeField]
    private Resource.Amount[] _cost;
    public Resource.Amount[] Cost => _cost;

    [Space]
    [Separator("UI")]

    [SerializeField]
    private Sprite _icon;
    public Sprite Icon => _icon;

    [SerializeField]
    private Color _color;
    public Color Color => _color;

    [SerializeField]
    private ToolTip _toolTip;
    public ToolTip ToolTip => _toolTip;

    [Space]
    [Separator("Art")]

    [SerializeField]
    private Sprite _characterSprite;
    public Sprite CharacterSprite => _characterSprite;

    [Space]
    [Separator("Fighting Stats")]

    [SerializeField]
    [Min(0)]
    private int _maxHealth;
    public int MaxHealth => _maxHealth;

    [Space]

    [SerializeField]
    [Min(0)]
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

    [Space]

    [SerializeField]
    [Min(0)]
    private int _meleeArmor;
    public int MeleeArmor => _meleeArmor;

    [SerializeField]
    [Min(0)] 
    private int _rangeArmor;
    public int RangeArmor => _rangeArmor;   
    
    [Space]

    [SerializeField]
    private Sprite _weapon;
    public Sprite Weapon => _weapon;

    [Space]
    [Separator("Economy Stats")]

    [SerializeField]
    private bool _canHarvestResources;
    public bool CanHarvestResources => _canHarvestResources;

    [ConditionalField(nameof(_canHarvestResources))]
    [SerializeField]
    private int _maxCarriedResources;
    public int MaxCarriedResources => _maxCarriedResources;

    [Space]

    [SerializeField]
    private bool _canBuild;
    public bool CanBuild => _canBuild;

    [ConditionalField(nameof(_canBuild))]
    [SerializeField]
    private int _buildEfficiencyMultiplier;
    public int BuildEfficiencyMultiplier => _buildEfficiencyMultiplier;
}
