using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RessourceType
{
    Coins,
    Plutonium,
    Water
}
public abstract class Ressource : TickedBehaviour
{

    [SerializeField]
    private RessourceData _data;

    public RessourceData Data => _data;

    /// <returns>The destination of the peon to cut the tree at <paramref name="treePosition"/>.</returns>
    public Vector2Int GetHarvestingPosition(Vector2Int treePosition)
    {
        Vector2Int currentPos = treePosition;
        List<Vector2Int> availableTiles = new();
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
                    Vector2Int tilePosition = currentPos + new Vector2Int(i, j);
                    if (TileMapManager.GetTile(tilePosition).State == TileState.Free)
                        availableTiles.Add(tilePosition);
                }
            }
            //If we found at least one candidate
            if (availableTiles.Count > 0)
            {
                //Find the nearest candidate to the tree in magnitude
                (int minMagnitude, int index) = ((availableTiles[0] - treePosition).sqrMagnitude, 0);
                for (int i = 1; i < availableTiles.Count; i++)
                {
                    int currentMagnitude = (availableTiles[i] - treePosition).sqrMagnitude;
                    if (currentMagnitude < minMagnitude)
                        (minMagnitude, index) = (currentMagnitude, i);
                    /*
                     * TODO : À magnitudes égales, choisir le plus proche du joueur via pathfinding (optionnel)  
                     * Remarque : S'il y a un trou proche de l'arbre cible mais inaccessible, c'est cette tile qui sera retenue
                     */
                }
                return availableTiles[index];
            }
        }
        Debug.LogError("No free tile found !");
        return treePosition;
    }
}
