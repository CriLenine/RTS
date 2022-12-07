using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResourceStorer
{
    public List<ResourceType> StorableResources { get; }
    public void Fill(ResourceType type, int amount)
    {
        GameManager.AddResource(type, amount);
    }
}
