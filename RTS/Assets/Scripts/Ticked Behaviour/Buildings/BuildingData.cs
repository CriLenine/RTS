using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;

[CreateAssetMenu(fileName = "BuildingData", menuName = "BuildingData", order = 1)]

public class BuildingData : ButtonData
{
    [Separator("Spawn Data")]
    [Space]

    [SerializeField]
    private Building.Type _type;
    public Building.Type Type => _type;

    [SerializeField]
    private SubType _subType;
    public SubType SubType => _subType;

    [SerializeField]
    private Building _building;
    public Building Building => _building;

    [SerializeField]
    [Min(1)]
    private int _size;
    public int Size => _size;

    [SerializeField]
    [Min(0)]
    private int _requiredBuildTicks;
    public int RequiredBuildTicks => _requiredBuildTicks;

    [SerializeField]
    private Resource.Amount[] _cost;
    public Resource.Amount[] Cost => _cost;

    [Separator("Game Specs")]
    [Space]

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

    [Separator("Resources Outpost Specs")]
    [Space]

    [SerializeField]
    private bool _canCollectResources = false;
    public bool CanCollectResources => _canCollectResources;

    [Space]

    [ConditionalField(nameof(_canCollectResources))]
    [SerializeField]
    private ResourceCollection _collectableResources;
    public ResourceCollection CollectableResources => _collectableResources;

    [Separator("UI")]
    [Space]

    [SerializeField]
    private Sprite Icon;
    public Sprite HUDIcon => Icon;

    [SerializeField]
    private Color _color;
    public Color Color => _color;

    [SerializeField]
    private ToolTip _toolTip;
    public ToolTip ToolTip => _toolTip;

    [SerializeField]
    private List<ButtonDataHUDParameters> _actions;
    public List<ButtonDataHUDParameters> Actions => _actions;

    [HideInInspector]
    public bool CanSpawnUnits = false;
}

[Serializable]
public class ResourceCollection : CollectionWrapper<ResourceType> { }
