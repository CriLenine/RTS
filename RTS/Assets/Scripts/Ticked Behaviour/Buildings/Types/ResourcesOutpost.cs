using System.Collections.Generic;
using UnityEngine;

public class ResourcesOutpost : Building, IResourceStorer
{
    private ResourcesOutpostData _resourcesOutPostData;
    public List<ResourceType> StorableResources => _resourcesOutPostData.StorableResources;

    private void Start()
    {
        _resourcesOutPostData = (ResourcesOutpostData)_buildingData;
    }
    public override Hash128 GetHash128()
    {
        return base.GetHash128();
    }

    public override void Tick() { }
}
