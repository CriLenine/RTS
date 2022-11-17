using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingData : ScriptableObject
{
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
}
