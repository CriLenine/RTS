using UnityEngine;

public class Housing : Building
{
    private void Start() { _type = Type.Housing; }

    public override Hash128 GetHash128() { return base.GetHash128(); }

    public override void Tick() { }
}
