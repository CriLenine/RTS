using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Peon", menuName = "Characters/Peon", order = 1)]
public class PeonData : CharacterData
{
    [SerializeField]
    private int _workPower;

    [SerializeField]
    private List<Building.Type> _buildable;

    public int WorkPower => _workPower;
    public List<Building.Type> Buildable => _buildable;
}
