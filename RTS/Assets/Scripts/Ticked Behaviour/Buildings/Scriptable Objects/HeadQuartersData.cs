using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Farm", menuName = "Buildings/HeadQuarters", order = 3)]
public class HeadQuartersData : BuildingData
{
    [SerializeField]
    private List<Character.Type> _spawnable;

    public List<Character.Type> Spawnable => _spawnable;
}
