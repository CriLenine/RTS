using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveBuildingData : BuildingData
{
    [SerializeField]
    [Min(0)]
    private int _range;
}
