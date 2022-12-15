using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingData : ScriptableObject
{

    [Header("Spawn Data")]

    [SerializeField]
    private Building.Type _type;
    public Building.Type Type => _type;

    [Space]
    [Space]

    [Header("HUD")]
    [SerializeField]
    private Resource.Amount[] _cost;
    public Resource.Amount[] Cost => _cost;

    [Space]
    [Space]

    [Header("Building")]

    [SerializeField]
    private Building _building;
    public Building Building => _building;

    [SerializeField]
    [Min(0)]
    private int _outline;
    public int Outline => _outline;

    [Space]
    [Space]

    [Header("UI")]

    [SerializeField]
    private Sprite _HUDIcon;
    public Sprite HUDIcon => _HUDIcon;

    [Space]
    [Space]

    [Header("Game Specs")]

    [SerializeField]
    [Min(0)]
    private int _requiredBuildTicks;
    public int RequiredBuildTicks => _requiredBuildTicks;

    [SerializeField]
    [Min(0)]
    private int _maxHealth;
    public int MaxHealth => _maxHealth;

    [SerializeField]
    [Min(0)]
    private int _housingProvided;
    public int HousingProvided => _housingProvided;

    [SerializeField]
    [Min(0)]
    private int _meleeArmor, _rangeArmor;
    public int MeleeArmor => _meleeArmor;
    public int RangeArmor => _rangeArmor;

    [SerializeField]
    private Sprite[] _buildingProgressSprites;
    public Sprite[] BuildingProgressSprites => _buildingProgressSprites;
}
