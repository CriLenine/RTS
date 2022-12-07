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
    [SerializeField]
    private ResourceData _data;

    public ResourceData Data => _data;

    public abstract void Clear();

    public abstract Vector2Int GetTileToHarvest(Vector2Int coords);

    ///<summary>Called when a new resource tile is selected to be harvested.</summary>
    /// <returns>The destination for the worker to harvest the resource at <paramref name="resourcePosition"/>.
    /// If no suitable tile is found, returns <paramref name="resourcePosition"/>.</returns>
    public Vector2Int GetHarvestingPosition(Vector2Int resourcePosition, Vector2Int harvesterPosition)
    {
        List<Vector2Int> availableTiles = new List<Vector2Int>();
        //Check all the outlines around the tree
        for (int outline = 1; outline <= /*TileMapManager.MaxValue*/ 5; ++outline)
        {
            //Run through each tile of the current outline
            for (int i = -outline; i <= outline; ++i)
            {
                for (int j = -outline; j <= outline; ++j)
                {
                    if (i == 0 && j == 0)
                        continue;
                    Vector2Int tilePosition = resourcePosition + new Vector2Int(i, j);
                    if (TileMapManager.GetLogicalTile(tilePosition).IsFree(0)) // TODO : performer
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
