using UnityEngine;

public class Naked : Character
{
    private Building _workedOnBuilding;

    private Resource _recoltedRessource;

    private NakedData _specificData;

    public new NakedData Data => _specificData;

    public override bool Idle => _workedOnBuilding == null && _recoltedRessource == null;

    protected override void Start()
    {
        base.Start();

        SetType(Type.Naked);

        _specificData = (NakedData)_data;
    }

    public override Hash128 GetHash128()
    {
        return base.GetHash128();
    }

    public void SetBuild(Building building)
    {
        _workedOnBuilding = building;
    }
}