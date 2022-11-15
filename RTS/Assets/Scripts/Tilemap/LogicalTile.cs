
using UnityEngine;

public enum TileState
{
    Free,
    BuildingOutline,
    Obstacle,
}

public class LogicalTile
{
    public Vector2Int Coords;
    public TileState State = TileState.Free;
    public bool isObstacle => State == TileState.Obstacle;
    public bool isBuildable => State == TileState.Free;

    public LogicalTile(Vector2Int iCoords)
    {
        Coords = iCoords;
    }

    public float f => g + h;
    public float g, h;
    public LogicalTile parent;
}
