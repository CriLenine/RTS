using UnityEngine;

public class Farm : Building
{
    private void Start()
    {
        SetType(Type.Farm);
    }
    public override Hash128 GetHash128()
    {
        return base.GetHash128();
    }

    public override void Tick()
    {

    }
}
