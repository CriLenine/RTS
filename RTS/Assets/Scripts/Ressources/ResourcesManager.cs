using UnityEngine;
using UnityEngine.Tilemaps;

public class ResourcesManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap _treesTilemap;

    [SerializeField]
    private Tilemap _rocksTilemap;

    private Forest[] _forests;
    private Aggregate[] _aggregates;
    private Camp[] _camps;

    private void Start()
    {
        Bake();
    }

    public bool Harvestable(Vector2Int coords)
    {
        return HasTree(coords) && GetNearestForest(coords).IsHarvestable(coords) 
            || HasRock(coords) && GetNearestAggregate(coords).IsHarvestable(coords);
    }
    public bool HasTree(Vector2Int coords)
    {
        return _treesTilemap.HasTile(new Vector3Int(coords.x, coords.y));
    }
    public bool HasTree(Vector2 position)
    {
        return HasTree(new Vector2Int((int)position.x, (int)position.y));
    }
    public bool HasRock(Vector2Int coords)
    {
        return _rocksTilemap.HasTile(new Vector3Int(coords.x, coords.y));
    }
    public bool HasRock(Vector2 position)
    {
        return HasRock(new Vector2Int((int)position.x, (int)position.y));
    }

    public void RemoveTree(Vector2Int coords)
    {
        _treesTilemap.SetTile(new Vector3Int(coords.x, coords.y), null);
    }

    public void RemoveRock(Vector2Int coords)
    {
        _rocksTilemap.SetTile(new Vector3Int(coords.x, coords.y), null);
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

    public Forest GetNearestForest(Vector2Int position)
    {
        (int minMagnitude, int index) = (int.MaxValue, 0);

        for (int i = 0; i < _forests.Length; ++i)
        {
            int currentMagnitude = ((Vector2Int)_treesTilemap.WorldToCell(_forests[i].transform.position) - position).sqrMagnitude;

            if (currentMagnitude < minMagnitude)
                (minMagnitude, index) = (currentMagnitude, i);
        }

        return _forests[index];
    }

    public Aggregate GetNearestAggregate(Vector2Int position)
    {
        (int minMagnitude, int index) = (int.MaxValue, 0);

        for (int i = 0; i < _aggregates.Length; ++i)
        {
            int currentMagnitude = ((Vector2Int)_rocksTilemap.WorldToCell(_aggregates[i].transform.position) - position).sqrMagnitude;

            if (currentMagnitude < minMagnitude)
                (minMagnitude, index) = (currentMagnitude, i);
        }

        return _aggregates[index];
    }

    public Camp GetNearestCamp(CampEntity campEntity)
    {
        (float minMagnitude, int index) = (float.MaxValue, 0);

        for (int i = 0; i < _camps.Length; ++i)
        {
            float currentMagnitude = (_camps[i].transform.position - campEntity.Position).sqrMagnitude;

            if (currentMagnitude < minMagnitude)
                (minMagnitude, index) = (currentMagnitude, i);
        }

        return _camps[index];
    }
}
