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

    public Amount CurrentAmount;

    [SerializeField]
    private ResourceData _data;

    protected Dictionary<Vector2Int, int> _itemsNHarvested = new Dictionary<Vector2Int, int>();

    private readonly List<Vector2Int> _dirs = new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

    public ResourceData Data => _data;

    /// <summary>
    /// Checks if the <paramref name="coords"/> correspond to an item in this resource, and if it still holds something.
    /// </summary>
    public bool IsHarvestable(Vector2Int coords) => _itemsNHarvested.ContainsKey(coords);

    /// <summary>
    /// Called when a tile is harvested. Executes the needed operations on the tile.
    /// </summary>
    /// <param name="coords">The coords of the harvested tile</param>
    public abstract void OnHarvestedTile(Vector2Int coords);

    public void AddItem(Vector2Int newItem)
    { 
        _itemsNHarvested.Add(newItem, 0);
    }

    public void Clear()
    {
        _itemsNHarvested?.Clear();
    }

    public void Init()
    {
        CurrentAmount = new Amount(Data.Type, _itemsNHarvested.Count);
    }

    ///<summary>Called when a new resource tile is selected to be harvested.</summary>
    /// <returns>The closest tile from <paramref name="resourceCoords"/> allowing a harvest.
    /// If no suitable tile is found, returns <see langword="null"/>.</returns>
    public virtual Vector2Int? GetHarvestingPosition(Vector2Int resourceCoords, Vector2Int harvesterCoords, int performer)
    {
        LogicalTile destinationTile = TileMapManager.FindPathWithTag(performer, harvesterCoords, resourceCoords, this is Forest ? TileTag.Tree : TileTag.Rock, 10);
        if (destinationTile == null)
            Debug.Log("The pathfinding to the next tile failed.");

        return destinationTile?.Coords;
        #region OldFunction
        /*
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
        */
        #endregion
    }

    /// <summary>
    /// Called when a tile is harvested.
    /// </summary>
    /// <returns>The position of the next tile to harvest, or <see langword="null"/> if the resource is depleted or if no close suitable tile has been found.</returns>
    public virtual Vector2Int? GetNext(Vector2Int attractionPoint, Vector2Int characterCoords, int performer)
    {
        if (CurrentAmount.Value < 1)
        {
            Debug.Log("This resource is completely depleted.");
            return null;
        }
        /* Naive search around the last harvested tile */

        List<Vector2Int> availableTiles = new List<Vector2Int>();
        int outline = 0;
        const int MAX_OUTLINE_SEARCH = 10;
        while (++outline <= MAX_OUTLINE_SEARCH)
        {
            for (int i = -outline; i <= outline; ++i)
            {
                for (int j = -outline; j <= outline; ++j)
                {
                    if (i != -outline && i != outline && j != -outline && j != outline)
                        continue;

                    Vector2Int tileCoords = characterCoords + new Vector2Int(i, j);

                    if (_itemsNHarvested.ContainsKey(tileCoords)
                        && !IsSurrounded(tileCoords, performer)
                        && IsPath(characterCoords, tileCoords, performer))

                        availableTiles.Add(tileCoords);
                }
            }

            if (availableTiles.Count > 0)
                return FindClosestCoords(availableTiles, attractionPoint);
        }

        Debug.Log($"There is no havestable and accessible tile within a {MAX_OUTLINE_SEARCH} tiles square around {characterCoords}.");
        return null;
    }

    public void HarvestTile(Vector2Int lastHarvested)
    {
        if (_itemsNHarvested.ContainsKey(lastHarvested))
        {
            OnHarvestedTile(lastHarvested);
            if (++_itemsNHarvested[lastHarvested] >= Data.NMaxHarvestPerTile)
                _itemsNHarvested.Remove(lastHarvested);
        }
    }

    /// <summary>
    /// Get the closest harvestable tile to <paramref name="attractionPoint"/> among the 4 direct neighbors of <paramref name="coords"/>.
    /// </summary>
    /// <param name="coords">The coords around which search a harvestable tile</param>
    /// <param name="attractionPoint">The point used to orient the choice</param>
    /// <returns>The tile coordinates, or <see langword="null"/> if none of the 4 tiles were havestable.</returns>
    public virtual Vector2Int? GetTileToHarvest(Vector2Int coords, Vector2Int attractionPoint)
    {
        List<Vector2Int> availableTiles = new List<Vector2Int>();

        foreach (Vector2Int dir in _dirs)
            if (_itemsNHarvested.ContainsKey(coords + dir))
                availableTiles.Add(coords + dir);

        if (availableTiles.Count > 0)
            return FindClosestCoords(availableTiles, attractionPoint);

        //Debug.Log($"There's no harvestable tile from {coords}.");
        return null;
    }

    /// <summary>
    /// Finds the <paramref name="availableTiles"/>'s closest element to <paramref name="attractionPoint"/> in euclidean distance.
    /// </summary>
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

    /// <summary>
    /// Checks if a path exists from <paramref name="startCoords"/> to one of the 4 tiles right next to <paramref name="endCoords"/>.
    /// </summary>
    private bool IsPath(Vector2Int startCoords, Vector2Int endCoords, int performer)
    {
        foreach (Vector2Int dir in _dirs)
            if(TileMapManager.GetLogicalTile(endCoords + dir)?.IsFree(performer) == true
                && TileMapManager.FindPath(performer, startCoords, endCoords + dir) != null)
                return true;

        return false;
    }

    /// <summary>
    /// Checks if the tile at <paramref name="coords"/> is completely surrounded by obstacles.
    /// </summary>
    private bool IsSurrounded(Vector2Int coords, int performer)
    {
        foreach (Vector2Int dir in _dirs)
            if (TileMapManager.GetLogicalTile(coords + dir)?.IsFree(performer) == true)
                return false;

        return true;
    }

}