
using UnityEngine;

public enum TileState
{
    Free,
    BuildingOutline,
    Obstacle,
}

public class LogicalTile
{
    public Vector2Int coords;
    public TileState state = TileState.Free;
    public bool isObstacle => state == TileState.Obstacle;
    public bool isBuildable => state == TileState.Free;

    public LogicalTile(Vector2Int iCoords)
    {
        coords = iCoords;
    }

    public float f => g + h;
    public float g, h;
    public LogicalTile parent;
}
