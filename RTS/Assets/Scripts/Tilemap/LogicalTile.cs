using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileState
{
    Free,
    BuildingOutline,
    Obstacle,
    Tree
}

public class LogicalTile
{
    public int x, y;
    public TileState State = TileState.Free;

    public LogicalTile(int ix, int iy)
    {
        x = ix; 
        y = iy;
    }
}
