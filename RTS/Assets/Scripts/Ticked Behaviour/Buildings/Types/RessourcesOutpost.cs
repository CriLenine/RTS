using UnityEngine;

public class RessourcesOutpost : Building
{
    private void Start()
    {
        SetType(Type.Ressourcesoutpost);
    }
    public override Hash128 GetHash128()
    {
        throw new System.NotImplementedException();
    }

    public override void Tick()
    {
        throw new System.NotImplementedException();
    }
}
