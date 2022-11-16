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

    public int TotalWorkforce => _totalWorkforce;
    public int MaxHealth => _maxHealth;
}
