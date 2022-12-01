using System.Collections.Generic;
using UnityEngine;

public enum RessourceType
{
    Coins,
    Plutonium,
    Meat
}

public abstract class Ressource : MonoBehaviour
{
    [SerializeField]
    private RessourceData _data;

    public RessourceData Data => _data;

    public abstract void Clear();

    public abstract Vector2Int GetTileToHarvest(Vector2Int coords);

    ///<summary>Called when a new ressource tile is selected to be harvested.</summary>
    /// <returns>The destination for the worker to harvest the ressource at <paramref name="ressourcePosition"/>.
    /// If no suitable tile is found, returns <paramref name="ressourcePosition"/>.</returns>
    public Vector2Int GetHarvestingPosition(Vector2Int ressourcePosition, Vector2Int harvesterPosition)
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
                    Vector2Int tilePosition = ressourcePosition + new Vector2Int(i, j);
                    Debug.Log(tilePosition);
                    if (TileMapManager.GetLogicalTile(tilePosition).IsFree /*&& pathfindingOK*/)
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
        return ressourcePosition;
    }
}
