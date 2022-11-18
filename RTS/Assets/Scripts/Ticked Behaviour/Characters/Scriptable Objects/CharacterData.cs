using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterData : ScriptableObject
{
    [SerializeField]
    [Min(0)]
    private int _maxHealth;

    public int MaxHealth => _maxHealth;
}