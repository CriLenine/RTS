using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResourceStorer
{
    public List<ResourceType> StorableResources { get; }
    public void Fill(Resource.Amount _ressourceAmount, int performer)
    {
        if (performer == NetworkManager.Me)
            GameManager.AddResource(_ressourceAmount.Type, _ressourceAmount.Value);
    }
}
