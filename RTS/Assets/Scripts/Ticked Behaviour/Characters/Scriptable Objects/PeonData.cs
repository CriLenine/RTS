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

    [SerializeField]
    private List<Building.Type> _buildable;

    [SerializeField]
    private int _nMaxCarriedResources;

    public List<Building.Type> Buildable => _buildable;

    public int WorkPower => _workPower;

    public int NMaxCarriedResources => _nMaxCarriedResources;
}
