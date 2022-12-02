using UnityEngine;

public enum TileState
{
    Free,
    BuildingOutline,
    Obstacle
}

public class LogicalTile
{
    public TileState State = TileState.Obstacle;

    public bool[] Fog = new bool[4];

    public bool IsFree(int performer)
    {
        return State != TileState.Obstacle || IsFog(performer);
    }

    public void SetFog(bool state)
    {
        for (int i = 0; i < Fog.Length; ++i)
            Fog[i] = state;
    }

    public void SetFog(int performer, bool state)
    {
        Fog[performer] = state;
    }

    public bool IsFog(int performer)
    {
        return Fog[performer];
    }

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
