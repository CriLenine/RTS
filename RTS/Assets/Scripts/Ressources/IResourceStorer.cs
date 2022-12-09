using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResourceStorer
{
    public List<ResourceType> StorableResources { get; }
    public void Fill(Resource.Amount _ressourceAmount)
    {
        GameManager.AddResource(_ressourceAmount.Type, _ressourceAmount.Value);
    }
}
