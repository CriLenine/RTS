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
    private int _buildEfficiencyMultiplier;

    [SerializeField]
    private int _nMaxCarriedResources;

    public int BuildEfficiencyMultiplier => _buildEfficiencyMultiplier;

    public int NMaxCarriedResources => _nMaxCarriedResources;
}
