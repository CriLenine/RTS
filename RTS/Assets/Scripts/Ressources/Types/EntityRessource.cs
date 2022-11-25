using UnityEngine;

public class EntityRessource : Ressource
{
    public override Hash128 GetHash128()
    {
        return base.GetHash128();
    }

    public override void Tick()
    {
        throw new System.NotImplementedException();
    }
}
