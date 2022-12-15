using UnityEngine;

public class Housing : Building
{
    public override Hash128 GetHash128() { return base.GetHash128(); }

    public override void Tick() { }
}
