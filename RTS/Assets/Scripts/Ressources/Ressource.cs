using System.Collections.Generic;
using UnityEngine;

public enum RessourceType
{
    Coins,
    Plutonium,
    Meat
}

public abstract class Ressource : TickedBehaviour
{
    [SerializeField]
    private RessourceData _data;

    public RessourceData Data => _data;

    ///<summary>Called when a new ressource tile is selected to be harvested.</summary>
    /// <returns>The destination for the worker to harvest the ressource at <paramref name="ressourcePosition"/>.
    /// If no suitable tile is found, returns <paramref name="ressourcePosition"/>.</returns>
    public Vector2Int GetHarvestingPosition(Vector2Int ressourcePosition)
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
                    if (TileMapManager.GetLogicalTile(tilePosition).IsFree)
                        availableTiles.Add(tilePosition);
                }
            }
            //If we found at least one candidate
            if (availableTiles.Count > 0)
            {
                //Find the nearest candidate to the tree in magnitude
                (int minMagnitude, int index) = ((availableTiles[0] - ressourcePosition).sqrMagnitude, 0);
                for (int i = 1; i < availableTiles.Count; i++)
                {
                    int currentMagnitude = (availableTiles[i] - ressourcePosition).sqrMagnitude;
                    if (currentMagnitude < minMagnitude)
                        (minMagnitude, index) = (currentMagnitude, i);
                    /*
                     * TODO : � magnitudes �gales, choisir le plus proche du joueur via pathfinding (optionnel)  
                     * Remarque : S'il y a un trou proche de l'arbre cible mais inaccessible, c'est cette tile qui sera retenue
                     */
                }
                return availableTiles[index];
            }
        }
        Debug.LogError("No free tile found !");
        return ressourcePosition;
    }
}
