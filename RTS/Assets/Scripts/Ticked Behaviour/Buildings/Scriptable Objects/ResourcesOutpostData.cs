﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourcesOutpost", menuName = "Buildings/ReosourcesOutpost", order = 1)]
public class ResourcesOutpostData : BuildingData
{
    [SerializeField]
    private List<ResourceType> _storableResources;

    public List<ResourceType> StorableResources => _storableResources;
}
