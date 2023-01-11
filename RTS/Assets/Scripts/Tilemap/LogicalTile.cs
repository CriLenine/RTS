using UnityEngine;

public enum TileState
{
    Free,
    BuildingOutline,
    Obstacle
}

public enum TileTag
{
    None,
    Tree,
    Rock,
    Gold,
    Crystal
}

public class LogicalTile
{
    public TileState State = TileState.Obstacle;
    public TileTag Tag = TileTag.None;

    public readonly bool[] LastState = new bool[4];

    public bool IsFree(int performer)
    {
        return !LastState[performer];
    }

    public bool IsObstacle(int performer)
    {
        return LastState[performer];
    }

    public void Update()
    {
        for (int i = 0; i < LastState.Length; ++i)
            LastState[i] = State == TileState.Obstacle;
    }

    public void Update(int performer)
    {
        LastState[performer] = State == TileState.Obstacle;
    }

    public void Reset()
    {
        for (int i = 0; i < LastState.Length; ++i)
            LastState[i] = false;
    }

    public LogicalTile(Vector2Int coords, TileState state, TileTag tag)
    {
        Coords = coords;
        State = state;
        Tag = tag;

        Reset();
    }

    public Vector2Int Coords;
    public float F => G + H;
    public float G, H;
    public float Weight;
    public bool Visited;
    public LogicalTile Parent;
}
