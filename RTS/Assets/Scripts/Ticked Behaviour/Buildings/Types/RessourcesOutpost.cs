using System.Collections.Generic;
using UnityEngine;

public class RessourcesOutpost : Building, IRessourceStorer
{
    private void Start()
    {
        SetType(Type.RessourcesOutpost);
    }
    public override Hash128 GetHash128()
    {
        return base.GetHash128();
    }

    public override void Tick()
    {
        
    }
}
