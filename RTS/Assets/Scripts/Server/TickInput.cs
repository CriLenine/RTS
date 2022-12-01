using UnityEngine;

public enum InputType
{
    Spawn,
    Move,
    Build,
    Harvest,
    Hunt
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

    public static TickInput Build(int id, Vector2 position, int[] targets, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.Build,

            Targets = targets,

            ID = id,

            Position = position,
            Performer = performer
        };
    }

    public static TickInput Harvest(Vector2 position, int target, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.Harvest,

            Targets = new int[1] { target },

            Position = position,
            Performer = performer
        };
    }

    //public static TickInput Hunt(int id, int[] targets, int performer = 0)
    //{
    //    return new TickInput()
    //    {
    //        Type = InputType.Build,

    //        Targets = targets,

    //        ID = id,

    //        Performer = performer
    //    };
    //}
}
