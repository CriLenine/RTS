using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Farm", menuName = "Buildings/Farms", order = 1)]
public class FarmData : BuildingData
{
    [SerializeField]
    private List<Character.Type> _spawnable;

    public List<Character.Type> Spawnable => _spawnable;
}
