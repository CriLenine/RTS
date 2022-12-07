using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRessourceStorer
{
    public void Fill(RessourceType type, int amount)
    {
        GameManager.AddRessource(type, amount);
    }
}
