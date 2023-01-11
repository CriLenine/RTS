using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class ResourcesManager : MonoBehaviour
{
    private static ResourcesManager _instance;

    [SerializeField]
    private Tilemap _obstaclesTilemap;

    [SerializeField]
    private Tilemap _treesTilemap;

    [SerializeField]
    private Tilemap _rocksTilemap;

    [SerializeField]
    private Tilemap _goldTilemap;

    [SerializeField]
    private Tilemap _crystalsTilemap;

    private Forest[] _forests;
    private Aggregate[] _aggregates;

    private void Awake()
    {
        _instance ??= this;
    }

    private void Start()
    {
        Bake();
    }

    public static bool Harvestable(Vector2Int coords)
    {
        return HasTree(coords) && GetNearestForest(coords, ResourceType.Wood).IsHarvestable(coords) 
            || HasRock(coords) && GetNearestForest(coords, ResourceType.Stone).IsHarvestable(coords)
            || HasGold(coords) && GetNearestAggregate(coords, ResourceType.Gold).IsHarvestable(coords)
            || HasCrystal(coords) && GetNearestAggregate(coords, ResourceType.Crystal).IsHarvestable(coords);
    }

    #region HasResource
    public static bool HasTree(Vector2Int coords)
    {
        return _instance._treesTilemap.HasTile(new Vector3Int(coords.x, coords.y));
    }
    public static bool HasTree(Vector2 position)
    {
        return HasTree(new Vector2Int((int)position.x, (int)position.y));
    }

    public static bool HasRock(Vector2Int coords)
    {
        return _instance._rocksTilemap.HasTile(new Vector3Int(coords.x, coords.y));
    }
    public static bool HasRock(Vector2 position)
    {
        return HasRock(new Vector2Int((int)position.x, (int)position.y));
    }

    public static bool HasGold(Vector2Int coords)
    {
        return _instance._goldTilemap.HasTile(new Vector3Int(coords.x, coords.y));
    }
    public static bool HasGold(Vector2 position)
    {
        return HasGold(new Vector2Int((int)position.x, (int)position.y));
    }

    public static bool HasCrystal(Vector2Int coords)
    {
        return _instance._crystalsTilemap.HasTile(new Vector3Int(coords.x, coords.y));
    }
    public static bool HasCrystal(Vector2 position)
    {
        return HasCrystal(new Vector2Int((int)position.x, (int)position.y));
    }
    #endregion

    public static void UpdateTile(Vector2Int coords, ResourceType type, TileBase[] tileAspects, bool remove, float harvestProgression)
    {
        Tilemap tilemap = null;
        if (type == ResourceType.Wood) tilemap = _instance._treesTilemap;
        else if (type == ResourceType.Stone) tilemap = _instance._rocksTilemap;
        else if (type == ResourceType.Gold) tilemap = _instance._goldTilemap;
        else if (type == ResourceType.Crystal) tilemap = _instance._crystalsTilemap;

        tilemap.SetTile(new Vector3Int(coords.x, coords.y), remove ? null : tileAspects[Mathf.CeilToInt((tileAspects.Length - 1) * harvestProgression)]);
    }

    public static void RemoveRock(Vector2Int coords)
    {
        _instance._rocksTilemap.SetTile(new Vector3Int(coords.x, coords.y), null);
    }

    /// <summary>
    /// Dispatch every resource tile in the appropriate <see cref="Forest"/> or <see cref="Aggregate"/> 
    /// found in the scene.
    /// </summary>
    private void Bake()
    {
        _forests = FindObjectsOfType<Forest>();
        _aggregates = FindObjectsOfType<Aggregate>();

        if (_forests.Length + _aggregates.Length < 1)
            return;

        foreach (Forest forest in _forests)
            forest.Clear();
        foreach (Aggregate aggregate in _aggregates)
            aggregate.Clear();

        foreach (Vector3Int position in _obstaclesTilemap.cellBounds.allPositionsWithin)
        {
            Vector2Int pos2d = (Vector2Int)position;

            if (_treesTilemap.HasTile(position))
                GetNearestForest(pos2d, ResourceType.Wood).AddItem(pos2d);
            else if (_rocksTilemap.HasTile(position))
                GetNearestForest(pos2d, ResourceType.Stone).AddItem(pos2d);
            else if (_goldTilemap.HasTile(position))
                GetNearestAggregate(pos2d, ResourceType.Gold).AddItem(pos2d);
            else if (_crystalsTilemap.HasTile(position))
                GetNearestAggregate(pos2d, ResourceType.Crystal).AddItem(pos2d);
        }

        foreach (Forest forest in _forests)
            forest.Init();
        foreach (Aggregate aggregate in _aggregates)
            aggregate.Init();
    }

    public static Forest GetNearestForest(Vector2Int position, ResourceType forestType)
    {
        (int minMagnitude, int index) = (int.MaxValue, 0);

        for (int i = 0; i < _instance._forests.Length; ++i)
        {
            if (_instance._forests[i].Data.Type != forestType)
                continue;

            int currentMagnitude = ((Vector2Int)_instance._obstaclesTilemap.WorldToCell(_instance._forests[i].transform.position) - position).sqrMagnitude;

            if (currentMagnitude < minMagnitude)
                (minMagnitude, index) = (currentMagnitude, i);
        }

        return _instance._forests[index];
    }

    public static Aggregate GetNearestAggregate(Vector2Int position, ResourceType aggregateType)
    {
        (int minMagnitude, int index) = (int.MaxValue, 0);

        for (int i = 0; i < _instance._aggregates.Length; ++i)
        {
            if (_instance._aggregates[i].Data.Type != aggregateType)
                continue;

            int currentMagnitude = ((Vector2Int)_instance._obstaclesTilemap.WorldToCell(_instance._aggregates[i].transform.position) - position).sqrMagnitude;

            if (currentMagnitude < minMagnitude)
                (minMagnitude, index) = (currentMagnitude, i);
        }

        return _instance._aggregates[index];
    }
}
