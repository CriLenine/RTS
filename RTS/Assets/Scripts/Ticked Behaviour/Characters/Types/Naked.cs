using UnityEngine;

public class Naked : Character
{
    private Building _buildingCurrentlyBuilt;

    private Resource _recoltedResource;

    private NakedData _specificData;

    public new NakedData Data => _specificData;

    public override bool Idle => _buildingCurrentlyBuilt == null && _recoltedResource == null;

    protected override void Start()
    {
        base.Start();

        _type = Type.Naked;

        _specificData = (NakedData)_data;
    }

    public override Hash128 GetHash128()
    {
        return base.GetHash128();
    }

    public void SetBuild(Building building)
    {
        _buildingCurrentlyBuilt = building;
    }
}