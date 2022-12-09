using UnityEngine;

[CreateAssetMenu(fileName = "Peon", menuName = "Characters/Peon", order = 1)]
public class PeonData : CharacterData
{
    [SerializeField]
    private int _workPower;

    [SerializeField]
    private int _nMaxCarriedResources;

    public int WorkPower => _workPower;

    public int NMaxCarriedResources => _nMaxCarriedResources;
}
