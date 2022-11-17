using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RessourcesManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap _treesTilemap;

    private List<Vector2Int> _allTrees;

    [ContextMenu("Bake")]
    public void Bake()
    {
        ForestRessource[] forests = FindObjectsOfType<ForestRessource>();

        if (forests.Length < 1)
            return;

        foreach (ForestRessource forest in forests)
            forest.Clear();

        foreach (Vector3Int position in _treesTilemap.cellBounds.allPositionsWithin)
        {
            if (_treesTilemap.HasTile(position))
                FindNearestForest(forests, (Vector2Int)position).AddTree((Vector2Int)position);
        }

        foreach (ForestRessource forest in forests)
            forest.Bake((Vector2Int)_treesTilemap.WorldToCell(forest.transform.position));

        Debug.Log("Bake done");
    }

    private ForestRessource FindNearestForest(ForestRessource[] forests, Vector2Int position)
    {
        (int minMagnitude, int index) = (int.MaxValue, 0);

        for (int i = 0; i < forests.Length; ++i)
        {
            int currentMagnitude = ((Vector2Int)_treesTilemap.WorldToCell(forests[i].transform.position) - position).sqrMagnitude;

            if (currentMagnitude < minMagnitude)
                (minMagnitude, index) = (currentMagnitude, i);
        }

        return forests[index];
    }
}
