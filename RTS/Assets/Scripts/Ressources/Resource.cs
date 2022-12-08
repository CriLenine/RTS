using System;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    Coins,
    Plutonium,
    Meat
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

    [SerializeField]
    protected Dictionary<Vector2Int, int> _items = new Dictionary<Vector2Int, int>();

    public void AddItem(Vector2Int newItem) => _items.Add(newItem, 0);

    [SerializeField]
    private ResourceData _data;

    public ResourceData Data => _data;

    public Amount CurrentAmount;

    public void Clear()
    {
        _items?.Clear();
    }

    public void Init()
    {
        CurrentAmount = new Amount(Data.Type, _items.Count);
    }

    public abstract void HarvestedTile(Vector2Int coords);

    /// <summary>
    /// Called when a tile is harvested.
    /// </summary>
    /// <returns>The position of the next tile to harvest, or <see langword="null"/> if no available tile has been found.</returns>
    public Vector2Int? GetNext(Vector2Int lastHarvested)
    {
        HarvestedTile(lastHarvested);
        if (++_items[lastHarvested] >= Data.NMaxHarvestPerTile)
            _items.Remove(lastHarvested);
        if (CurrentAmount.Value < 1)
            return null;
        List<Vector2Int> availableTiles = new List<Vector2Int>();
        foreach (Vector2Int harvestableTile in GetHarvestableTiles(lastHarvested))
        {
            /*if (TileMapManager.FindPath(NetworkManager.Me, lastTree, tileCoords).Count > 0)*/
            availableTiles.Add(harvestableTile);
        }
        //If we found at least one candidate
        if (availableTiles.Count > 0)
        {
            //Find the nearest candidate in magnitude
            (int minMagnitude, int index) = ((availableTiles[0] - lastHarvested).sqrMagnitude, 0);
            for (int i = 1; i < availableTiles.Count; i++)
            {
                int currentMagnitude = (availableTiles[i] - lastHarvested).sqrMagnitude;
                if (currentMagnitude < minMagnitude)
                    (minMagnitude, index) = (currentMagnitude, i);
            }
            return availableTiles[index];
        }
        return null;
    }

    public Vector2Int GetTileToHarvest(Vector2Int coords)
    {
        for (int i = -1; i < 2; ++i)
        {
            for (int j = -1; j < 2; ++j)
            {
                Vector2Int tileCoords = coords + new Vector2Int(i, j);
                if (_items.TryGetValue(tileCoords, out int nHarvested))
                    if (nHarvested < Data.NMaxHarvestPerTile)
                        return coords + new Vector2Int(i, j);
            }
        }
        Debug.LogError("No tile found to harvest");
        return coords;
    }

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
    /// <returns>The destination for the worker to harvest the resource at <paramref name="resourcePosition"/>.
    /// If no suitable tile is found, returns <paramref name="resourcePosition"/>.</returns>
    public Vector2Int GetHarvestingPosition(Vector2Int resourcePosition, Vector2Int harvesterPosition)
    {
        List<Vector2Int> availableTiles = new List<Vector2Int>();
        //Check all the outlines around the tree
        for (int outline = 1; outline <= 5; ++outline)
        {
            //Run through each tile of the current outline
            for (int i = -outline; i <= outline; i += 2 * outline)
            {
                for (int j = -outline; j <= outline; ++j)
                {
                    if (i != -outline && i != outline && j != outline && j != outline)
                        continue;
                    Vector2Int tilePosition = resourcePosition + new Vector2Int(i, j);
                    if (TileMapManager.GetLogicalTile(tilePosition).IsFree(NetworkManager.Me) 
                        /*&& TileMapManager.FindPath(NetworkManager.Me, harvesterPosition, resourcePosition).Count > 0*/)
                        availableTiles.Add(tilePosition);
                }
            }
            //If we found at least one candidate
            if (availableTiles.Count > 0)
            {
                //Find the nearest candidate to the harvester in magnitude
                (int minMagnitude, int index) = ((availableTiles[0] - harvesterPosition).sqrMagnitude, 0);
                for (int i = 1; i < availableTiles.Count; i++)
                {
                    int currentMagnitude = (availableTiles[i] - harvesterPosition).sqrMagnitude;
                    if (currentMagnitude < minMagnitude)
                        (minMagnitude, index) = (currentMagnitude, i);
                }
                return availableTiles[index];
            }
        }
        Debug.LogError("No free tile found !");
        return resourcePosition;
    }
}
