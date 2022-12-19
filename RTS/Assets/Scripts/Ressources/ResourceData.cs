using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Resource", menuName = "Resource", order = 1)]
public class ResourceData : ScriptableObject
{
    [SerializeField]
    private ResourceType _type;

    [SerializeField]
    private int _amount;

    [SerializeField]
    private float _harvestingTime;

    [SerializeField]
    private int _amountPerHarvest;

    [SerializeField]
    private int _nMaxHarvestPerTile;

    public ResourceType Type => _type;

    public int Amount => _amount;

    public float HarvestingTime => _harvestingTime;

    public int AmountPerHarvest => _amountPerHarvest;

    public int NMaxHarvestPerTile => _nMaxHarvestPerTile;
}
