using UnityEngine;

public class Barracks : Building
{
    private void Start()
    {
        SetType(Type.Barracks);
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
