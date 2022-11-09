using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public int CurrentHealth
    {
        get
        {
            return CurrentHealth;
        }
        private set
        {
            CurrentHealth = value;
        }
    }
    public abstract int MaxHealth
    {
        get;
    }
    /// <returns><see langword="true"/> if health falls to zero</returns>
    public bool TakeDamage(int damage)
    {
        return (CurrentHealth -= damage) <= 0;
    }
    public void GainHealth(int amount)
    {
        if ((CurrentHealth += amount) > MaxHealth)
            CurrentHealth = MaxHealth;
    }
}
