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

    private void Start()
    {
        _specificData = (PeonData)_data;
    }

    protected override Hash128 GetHash128()
    {
        return new Hash128();
    }

    protected override void Tick()
    {
        if (_workedOnBuilding != null)
        {
            if (_workedOnBuilding.AddWorkforce(_specificData.WorkPower))
                _workedOnBuilding = null;
        }
    }
}
