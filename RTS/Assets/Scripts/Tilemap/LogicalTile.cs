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
    public TileState State = TileState.Obstacle;
    public bool IsObstacle => State == TileState.Obstacle;
    public bool IsFree => State == TileState.Free;
    public LogicalTile(Vector2Int coords, TileState state)
    {
        Coords = coords;
        State = state;
    }

    public Vector2Int Coords;
    public float f => g + h;
    public float g, h;
    public LogicalTile Parent;
}
