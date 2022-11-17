using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Peon : Character
{
    private Building _workedOnBuilding;

    private Ressource _recoltedRessource;

    private PeonData _specificData;

    public new PeonData Data => _specificData;

    public override bool Idle => _workedOnBuilding == null && _recoltedRessource == null;

    protected override void Start()
    {
        base.Start();
        _specificData = (PeonData)_data;
    }

    protected override Hash128 GetHash128()
    {
        return new Hash128();
    }

    public override void Tick()
    {
        if (_workedOnBuilding != null)
        {
            if (_workedOnBuilding.AddWorkforce(_specificData.WorkPower))
                _workedOnBuilding = null;
        }
    }

    public void SetBuild(Building building)
    {
        _workedOnBuilding = building;
    }
}
