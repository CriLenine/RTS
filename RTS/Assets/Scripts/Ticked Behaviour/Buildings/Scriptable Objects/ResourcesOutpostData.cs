using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourcesOutpost", menuName = "Buildings/ResourcesOutpost", order = 2)]
public class ResourcesOutpostData : BuildingData
{
    [Space]
    [Space]
    [Header("Resources Outpost Specs")]

    [SerializeField]
    private List<ResourceType> _storableResources;
    public List<ResourceType> StorableResources => _storableResources;
}
