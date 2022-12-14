using UnityEngine;
using UnityEngine.Tilemaps;

public class ResourcesManager : MonoBehaviour
{
    private static ResourcesManager _instance;

    [SerializeField]
    private Tilemap _treesTilemap;

    [SerializeField]
    private Tilemap _rocksTilemap;

    private Forest[] _forests;
    private Aggregate[] _aggregates;
    private Camp[] _camps;

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
        return HasTree(coords) && GetNearestForest(coords).IsHarvestable(coords) 
            || HasRock(coords) && GetNearestAggregate(coords).IsHarvestable(coords);
    }
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

    public static void RemoveTree(Vector2Int coords)
    {
        _instance._treesTilemap.SetTile(new Vector3Int(coords.x, coords.y), null);
    }

    public static void RemoveRock(Vector2Int coords)
    {
        _instance._rocksTilemap.SetTile(new Vector3Int(coords.x, coords.y), null);
    }

    /// <summary>
    /// Dispatch every resource tile in the appropriate <see cref="Forest"/>, <see cref="Camp"/> or <see cref="Aggregate"/> 
    /// found in the scene.
    /// </summary>
    private void Bake()
    {
        _forests = FindObjectsOfType<Forest>();
        _aggregates = FindObjectsOfType<Aggregate>();
        _camps = FindObjectsOfType<Camp>();

        Resource[] resources = new Resource[_forests.Length + _aggregates.Length + _camps.Length];

        if (resources.Length < 1)
            return;

        _forests.CopyTo(resources, 0);
        _aggregates.CopyTo(resources, _forests.Length);
        _camps.CopyTo(resources, _forests.Length + _aggregates.Length);

        foreach (Resource resource in resources)
            resource.Clear();

        foreach (Vector3Int position in _treesTilemap.cellBounds.allPositionsWithin)
        {
            Vector2Int pos2d = (Vector2Int)position;
            if (_treesTilemap.HasTile(position))
                GetNearestForest(pos2d).AddItem(pos2d);
            else if (_rocksTilemap.HasTile(position))
                GetNearestAggregate(pos2d).AddItem(pos2d);
        }

        foreach (CampEntity campEntity in FindObjectsOfType<CampEntity>())
            GetNearestCamp(campEntity).AddEntity(campEntity);

        foreach (Resource resource in resources)
            resource.Init();
    }

    public static Forest GetNearestForest(Vector2Int position)
    {
        (int minMagnitude, int index) = (int.MaxValue, 0);

        for (int i = 0; i < _instance._forests.Length; ++i)
        {
            int currentMagnitude = ((Vector2Int)_instance._treesTilemap.WorldToCell(_instance._forests[i].transform.position) - position).sqrMagnitude;

            if (currentMagnitude < minMagnitude)
                (minMagnitude, index) = (currentMagnitude, i);
        }

        return _instance._forests[index];
    }

    public static Aggregate GetNearestAggregate(Vector2Int position)
    {
        (int minMagnitude, int index) = (int.MaxValue, 0);

        for (int i = 0; i < _instance._aggregates.Length; ++i)
        {
            int currentMagnitude = ((Vector2Int)_instance._rocksTilemap.WorldToCell(_instance._aggregates[i].transform.position) - position).sqrMagnitude;

            if (currentMagnitude < minMagnitude)
                (minMagnitude, index) = (currentMagnitude, i);
        }

        return _instance._aggregates[index];
    }

    public static Camp GetNearestCamp(CampEntity campEntity)
    {
        (float minMagnitude, int index) = (float.MaxValue, 0);

        for (int i = 0; i < _instance._camps.Length; ++i)
        {
            float currentMagnitude = (_instance._camps[i].transform.position - campEntity.Position).sqrMagnitude;

            if (currentMagnitude < minMagnitude)
                (minMagnitude, index) = (currentMagnitude, i);
        }

        return _instance._camps[index];
    }
}
