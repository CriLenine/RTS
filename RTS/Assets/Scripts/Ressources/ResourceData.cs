using UnityEngine;

[CreateAssetMenu(fileName = "Resource", menuName = "Resource", order = 1)]
public class ResourceData : ScriptableObject
{
    [SerializeField]
    private ResourceType _type;

    [SerializeField]
    private float _harvestingTime;

    [SerializeField]
    private int _amountPerHarvest;

    [SerializeField]
    private int _nMaxHarvestPerTile;

    private int _amount;
    public ResourceType Type => _type;

    public float HarvestingTime => _harvestingTime;

    public int AmountPerHarvest => _amountPerHarvest;

    public int NMaxHarvestPerTile => _nMaxHarvestPerTile;
}
