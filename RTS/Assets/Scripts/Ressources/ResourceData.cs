using UnityEngine;
using UnityEngine.Tilemaps;

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

    [SerializeField]
    private TileBase[] _tileAspects;

    public ResourceType Type => _type;

    public float HarvestingTime => _harvestingTime;

    public int AmountPerHarvest => _amountPerHarvest;

    public int NMaxHarvestPerTile => _nMaxHarvestPerTile;

    public TileBase[] TileAspects => _tileAspects;
}
