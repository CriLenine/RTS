using System.Collections.Generic;
using UnityEngine;
using System;

public class Forest : Resource
{
    [SerializeField]
    private List<Vector2Int> _trees;

    private Node<Vector2Int> _holyNode;

    #region DEBUG
    public ResourcesManager resourcesManager;
    #endregion

    public int NTrees => _trees.Count;

    public void AddTree(Vector2Int newTree) => _trees.Add(newTree);

    public override void Clear()
    {
        _trees?.Clear();
    }

    private void Start()
    {
        resourcesManager = FindObjectOfType<ResourcesManager>();
    }

    public override Vector2Int GetTileToHarvest(Vector2Int coords)
    {
        for (int i = -1; i < 2; ++i)
        {
            for (int j = -1; j < 2; ++j)
            {
                if(_trees.Contains(coords + new Vector2Int(i, j)))
                    return coords + new Vector2Int(i, j);
            }
        }
        Debug.LogError("No tile found to harvest");
        return coords;
    }

    /// <summary>
    /// Called when a tree is cut.
    /// Removes the tree from the tilemap.
    /// </summary>
    /// <returns>The position of the next tree to cut, or <paramref name="lastTree"/> if forest is empty</returns>
    public Vector2Int GetNextTree(Vector2Int lastTree)
    {
        resourcesManager.RemoveTree(lastTree);
        _trees.Remove(lastTree);
        if (_trees.Count < 1)
            return lastTree;
        List<Vector2Int> availableTiles = new List<Vector2Int>();
        for (int i = -3; i <= 3; ++i)
        {
            for (int j = -3; j <= 3; ++j)
            {
                if (i == 0 && j == 0)
                    continue;
                Vector2Int tileCoords = lastTree + new Vector2Int(i, j);
                if (resourcesManager.HasTree(tileCoords) && 
                    TileMapManager.FindPath(NetworkManager.Me, lastTree, tileCoords).Count > 0)
                    availableTiles.Add(tileCoords);
            }
        }
        //If we found at least one candidate
        if (availableTiles.Count > 0)
        {
            //Find the nearest candidate to the tree in magnitude
            (int minMagnitude, int index) = ((availableTiles[0] - lastTree).sqrMagnitude, 0);
            for (int i = 1; i < availableTiles.Count; i++)
            {
                int currentMagnitude = (availableTiles[i] - lastTree).sqrMagnitude;
                if (currentMagnitude < minMagnitude)
                    (minMagnitude, index) = (currentMagnitude, i);
            }
            return availableTiles[index];
        }
        return lastTree;
    }
}
