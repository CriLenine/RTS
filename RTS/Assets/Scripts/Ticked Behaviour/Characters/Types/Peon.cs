using System.Collections.Generic;
using UnityEngine;

public class Peon : Character
{
    private Building _workedOnBuilding;

    private Resource _harvestedresource;

    private PeonData _specificData;

    public new PeonData Data => _specificData;

    public override bool Idle => _workedOnBuilding == null && _harvestedresource == null;

    public (ResourceType, int) CarriedResource;

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

    public void Setresource(Resource harvestedresource)
    {
        _harvestedresource = harvestedresource;
    }
}