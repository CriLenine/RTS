using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RessourcesManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap _treesTilemap;

    [SerializeField]
    private Tilemap _rocksTilemap;

    private Forest[] _forests;
    private Aggregate[] _aggregates;
    private Camp[] _camps;

    public GameObject prefab;
    public Forest forest;
    private void Start()
    {
        Bake();
        StartCoroutine(CutTree());
    }

    IEnumerator CutTree()
    {
        Vector2Int currentTree = new Vector2Int(22, 11);
        Vector2Int nextTree;
        do
        {
            Instantiate(prefab, _treesTilemap.CellToWorld(new Vector3Int(currentTree.x, currentTree.y, 0)) + new Vector3(0.25f, 0.25f, -1f), Quaternion.identity);
            nextTree = forest.GetNextTree(currentTree);
            if (nextTree == currentTree)
                break;
            currentTree = nextTree;
            yield return new WaitForSeconds(0.2f);
        } while (true);
    }

    public bool HasTree(Vector2Int coords)
    {
        return _treesTilemap.HasTile(new Vector3Int(coords.x, coords.y));
    }
    public bool HasTree(Vector2 position)
    {
        Vector2Int coords = (Vector2Int)_treesTilemap.WorldToCell(new Vector3(position.x, position.y));
        return HasTree(coords);
    }
    public bool HasRock(Vector2Int coords)
    {
        return _rocksTilemap.HasTile(new Vector3Int(coords.x, coords.y));
    }
    public bool HasRock(Vector2 position)
    {
        Vector2Int coords = (Vector2Int)_rocksTilemap.WorldToCell(new Vector3(position.x, position.y));
        return HasRock(coords);
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
    /// Dispatch every ressource tile in the appropriate <see cref="Forest"/>, <see cref="Camp"/> or <see cref="Aggregate"/> 
    /// found in the scene.
    /// </summary>
    private void Bake()
    {
        _forests = FindObjectsOfType<Forest>();
        _aggregates = FindObjectsOfType<Aggregate>();
        _camps = FindObjectsOfType<Camp>();

        Ressource[] ressources = new Ressource[_forests.Length + _aggregates.Length + _camps.Length];

        if (ressources.Length < 1)
            return;

        _forests.CopyTo(ressources, 0);
        _aggregates.CopyTo(ressources, _forests.Length);
        _camps.CopyTo(ressources, _forests.Length + _aggregates.Length);

        foreach (Ressource ressource in ressources)
            ressource.Clear();

        foreach (Vector3Int position in _treesTilemap.cellBounds.allPositionsWithin)
        {
            Vector2Int pos2d = (Vector2Int)position;
            if (_treesTilemap.HasTile(position))
                GetNearestForest(pos2d).AddTree(pos2d);
            else if (_rocksTilemap.HasTile(position))
                GetNearestAggregate(pos2d).AddRock(pos2d);
        }

        foreach (CampEntity campEntity in FindObjectsOfType<CampEntity>())
            GetNearestCamp(campEntity).AddEntity(campEntity);
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
