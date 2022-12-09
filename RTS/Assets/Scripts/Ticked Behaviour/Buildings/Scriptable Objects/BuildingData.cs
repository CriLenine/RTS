using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingData : ScriptableObject
{

    [Header("Spawn Data")]
    [Space]

    [SerializeField]
    private Building.Type _type;

    [Header("Building")]
    [SerializeField]
    private Blueprint _buildingBlueprint;

    [SerializeField]
    private Building _building;

    [SerializeField]
    [Min(0)]
    private int _outline;

    [Header("Data")]
    [SerializeField]
    private int _neededPlayerLevel;

    [SerializeField]
    private Resource.Amount[] _cost;

    [Header("UI")]
    [SerializeField]
    private Sprite _buildingUiIcon;

    public Building.Type Type => _type;
    public Blueprint BuildingBlueprint => _buildingBlueprint;
    public Building Building => _building;
    public int Outline => _outline;
    public Sprite BuildingUiIcon => _buildingUiIcon;


    [Header("Game Specs")]
    [Space]

    [SerializeField]
    [Min(0)]
    private int _totalWorkforce;

    [SerializeField]
    [Min(0)]
    private int _maxHealth;

    [SerializeField]
    private Sprite[] _constructionSteps;

    public int TotalWorkforce => _totalWorkforce;
    public int MaxHealth => _maxHealth;
    public Sprite[] ConstructionSteps => _constructionSteps;

    [SerializeField]
    private UtilsView[] _views;
    public UtilsView[] Views => _views;
}
