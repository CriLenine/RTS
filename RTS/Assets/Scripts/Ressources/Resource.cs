using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public enum ResourceType
{
    Crystal,
    Wood,
    Gold,
    Stone
}

public abstract class Resource : MonoBehaviour
{
    [Serializable]
    public struct Amount
    {
        public Amount(ResourceType type, int value = 0)
        {
            _type = type;
            _value = value;
        }

    [SerializeField]
        private ResourceType _type;

        [SerializeField]
        private int _value;

        public ResourceType Type => _type;

        public int Value => _value;

        public Amount AddQuantity(int quantity)
        {
            _value += quantity;
            return this;
        }
        public Amount RemoveQuantity(int quantity)
        {
            _value -= quantity;
            return this;
        }
    }

    protected Dictionary<Vector2Int, int> _items = new Dictionary<Vector2Int, int>();

    private readonly List<Vector2Int> _dirs = new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

    public void AddItem(Vector2Int newItem) => _items.Add(newItem, 0);

    public bool IsHarvestable(Vector2Int coords) => _items.ContainsKey(coords) && _items[coords] < Data.NMaxHarvestPerTile;

    [SerializeField]
    private ResourceData _data;

    public ResourceData Data => _data;

    public Amount CurrentAmount;


    /// <summary>
    /// Called when a tile is harvested. Executes the needed operations on the tile.
    /// </summary>
    /// <param name="coords">The coords of the harvested tile</param>
    public abstract void OnHarvestedTile(Vector2Int coords);
    public abstract void Bake();

    public void Clear()
    {
        _items?.Clear();
    }

    public void Init()
    {
        CurrentAmount = new Amount(Data.Type, _items.Count);
        Bake();
    }

    /// <summary>
    /// Called when a tile is harvested.
    /// </summary>
    /// <returns>The position of the next tile to harvest, or <see langword="null"/> if no available tile has been found.</returns>
    public virtual Vector2Int? GetNext(Vector2Int lastHarvested, Vector2Int attractionPoint, int performer)
    {
        OnHarvestedTile(lastHarvested);

        if (++_items[lastHarvested] >= Data.NMaxHarvestPerTile)
            _items.Remove(lastHarvested);

        if (CurrentAmount.Value < 1)
            return null;

        List<Vector2Int> availableTiles = new List<Vector2Int>();
        foreach (Vector2Int harvestableTile in GetHarvestableTiles(lastHarvested))
        {
            Stopwatch sw = Stopwatch.StartNew();
            if (IsPath(lastHarvested, harvestableTile, performer))
                availableTiles.Add(harvestableTile);
            Debug.Log($"IsPath took {sw.ElapsedMilliseconds} ms.");
        }

        //If we found at least one candidate
        if (availableTiles.Count > 0)
            return FindClosestCoords(availableTiles, attractionPoint);

        Debug.Log("No next suitable tile found.");
        return null;
    }

    /// <summary>
    /// Get a tile from this resource that can be harvested directly next to <paramref name="coords"/>.
    /// </summary>
    /// <param name="coords">The coords around which search a harvestable tile</param>
    /// <param name="attractionPoint">The point used to orient the choice</param>
    /// <returns>The harvestable coordinate which is closest to <paramref name="attractionPoint"/>.</returns>
    public virtual Vector2Int GetTileToHarvest(Vector2Int coords, Vector2Int attractionPoint)
    {
        List<Vector2Int> availableTiles = new List<Vector2Int>();

        foreach (Vector2Int dir in _dirs)
        {
            Vector2Int tileCoords = coords + dir;
            if (_items.TryGetValue(tileCoords, out int nHarvested))
                if (nHarvested < Data.NMaxHarvestPerTile)
                    availableTiles.Add(tileCoords);
        }

        //If we found at least one candidate
        if (availableTiles.Count > 0)
            return FindClosestCoords(availableTiles, attractionPoint);

        Debug.LogError("No tile found to harvest");
        return coords;
    }

    /// <summary>
    /// Get all the tiles in this resource in a small radius around <paramref name="coords"/>.
    /// </summary>
    /// <param name="coords"></param>
    /// <returns>A <see cref="List{Vector2Int}"/> containing the appropriate coords.</returns>
    public List<Vector2Int> GetHarvestableTiles(Vector2Int coords)
    {
        List<Vector2Int> harvestableTiles = new List<Vector2Int>();
        for (int i = -3; i < 3; ++i)
        {
            for (int j = -3; j < 3; ++j)
            {
                Vector2Int tileCoords = coords + new Vector2Int(i, j);
                if (_items.ContainsKey(tileCoords))
                    harvestableTiles.Add(tileCoords);
            }
        }
        return harvestableTiles;
    }

    ///<summary>Called when a new resource tile is selected to be harvested.</summary>
    /// <returns>The destination for the worker to harvest the resource at <paramref name="resourceCoords"/>.
    /// If no suitable tile is found, returns <paramref name="resourceCoords"/>.</returns>
    public virtual Vector2Int GetHarvestingPosition(Vector2Int resourceCoords, Vector2Int harvesterCoords, int performer)
    {
        List<Vector2Int> availableTiles = new List<Vector2Int>();
        //Check all the outlines around the tree
        for (int outline = 1; outline <= 5; ++outline)
        {
            for (int i = -outline; i <= outline; ++i)
            {
                for (int j = -outline; j <= outline; ++j)
                {
                    if (i != -outline && i != outline && j != -outline && j != outline)
                        continue;
                    Vector2Int tileCoords = resourceCoords + new Vector2Int(i, j);
                    if (TileMapManager.GetLogicalTile(tileCoords)?.IsFree(performer) == true
                        && TileMapManager.FindPath(performer, harvesterCoords, tileCoords)?.Count > 0
                        && IsValidHarvestPosition(tileCoords))
                        availableTiles.Add(tileCoords);
                }
            }

            //If we found at least one candidate
            if (availableTiles.Count > 0)
                return FindClosestCoords(availableTiles, resourceCoords);
        }

        Debug.Log("No free tile found !");
        return resourceCoords;
    }

    public static Vector2Int FindClosestCoords(List<Vector2Int> availableTiles, Vector2Int attractionPoint)
    {
        (int minMagnitude, int index) = ((availableTiles[0] - attractionPoint).sqrMagnitude, 0);
        for (int i = 1; i < availableTiles.Count; i++)
        {
            int currentMagnitude = (availableTiles[i] - attractionPoint).sqrMagnitude;
            if (currentMagnitude < minMagnitude)
                (minMagnitude, index) = (currentMagnitude, i);
        }
        return availableTiles[index];
    }

    private bool IsValidHarvestPosition(Vector2Int coords)
    {
        foreach (Vector2Int dir in _dirs)
            if (IsHarvestable(coords + dir))
                return true;
        return false;
    }

    private bool IsPath(Vector2Int startCoords, Vector2Int endCoords, int performer)
    {
        foreach (Vector2Int dir in _dirs)
            if (TileMapManager.FindPath(performer, startCoords, endCoords + dir) != null)
                return true;

        return false;
    }
}