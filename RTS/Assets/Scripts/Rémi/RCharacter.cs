using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RCharacter : TickedBehaviour, IDamageable
{
    [SerializeField]
    protected CharacterData _data;

    public CharacterData Data => _data;
    public int MaxHealth => _data.MaxHealth;
}
