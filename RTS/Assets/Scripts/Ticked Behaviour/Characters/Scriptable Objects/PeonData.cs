using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Peon", menuName = "Characters/Peon", order = 1)]
public class PeonData : CharacterData
{
    [Space]
    [Space]

    [Header("Economy Stats")]
    [Space]

    [SerializeField]
    private int _workPower;
    public int WorkPower => _workPower;
}
