using UnityEngine;

public class Peon : Character
{
    private Building _workedOnBuilding;

    private Resource _recoltedRessource;

    private PeonData _specificData;

    public new PeonData Data => _specificData;
    public Resource Resource => _recoltedRessource;

    public override bool Idle => _workedOnBuilding == null && _recoltedRessource == null;

    protected override void Start()
    {
        base.Start();

        SetType(Type.Peon);

        _specificData = (PeonData)_data;
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