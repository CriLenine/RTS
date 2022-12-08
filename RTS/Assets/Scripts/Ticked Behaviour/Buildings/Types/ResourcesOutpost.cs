using System.Collections.Generic;
using UnityEngine;

public class ResourcesOutpost : Building, IResourceStorer
{
    private ResourcesOutpostData _specificData;
    public new ResourcesOutpostData Data => _specificData;
    public List<ResourceType> StorableResources => _specificData.StorableResources;

    private void Start()
    {
        SetType(Type.PlutoniumOutpost);
        _specificData = (ResourcesOutpostData)_data;
    }
    public override Hash128 GetHash128()
    {
        return base.GetHash128();
    }

    public override void Tick()
    {
        
    }
}
