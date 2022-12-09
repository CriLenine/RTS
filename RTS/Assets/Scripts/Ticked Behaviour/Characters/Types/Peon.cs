using System.Collections.Generic;
using UnityEngine;

public class Peon : Character
{
    private Building _workedOnBuilding;

    private Resource _harvestedResource;

    private PeonData _specificData;

    public new PeonData Data => _specificData;
    public override bool Idle => _workedOnBuilding == null && _harvestedResource == null;

    public Resource.Amount CarriedResource;

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

    public void SetResource(Resource harvestedResource)
    {
        _harvestedResource = harvestedResource;
    }
}