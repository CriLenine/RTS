using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    /// <returns><see langword="true"/> if it dead,
    /// <see langword="false"/> otherwise </returns>
    public abstract bool TakeDamage(int damage);

    /// Tu connais
    public abstract void GainHealth(int amount);

}
