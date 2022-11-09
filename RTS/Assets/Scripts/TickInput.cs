using UnityEngine;

public enum InputType
{
    Spawn,
    Move,
    Build
}

public class TickInput
{
    public int Performer;

    public InputType Type;

    public int[] Targets;

    public int ID;

    public Vector2 Position;

    public static TickInput Spawn(int id, Vector2 position, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.Spawn,

            ID = id,

            Position = position,
            Performer = performer
        };
    }

    public static TickInput Move(int[] targets, Vector2 position, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.Move,

            Targets = targets,

            Position = position,
            Performer = performer
        };
    }

    public static TickInput Build(int id, Vector2 position, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.Build,

            ID = id,

            Position = position,
            Performer = performer
        };
    }
}
