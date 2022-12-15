using UnityEngine;

public enum InputType
{
    Spawn,
    Move,
    NewBuild,
    Build,
    Harvest,
    Hunt,
    Attack,
    Kill,
    GuardPosition,
    Destroy
}

public class TickInput
{
    public int Performer;

    public InputType Type;

    public int[] Targets;

    public int ID = -1;
    public int Prefab;

    public Vector2 Position;

    public static TickInput Spawn(int prefab, int spawnerID, Vector2 position, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.Spawn,

            ID = spawnerID,
            Prefab = prefab,

            Position = position,
            Performer = performer
        };
    }
    public static TickInput Kill(int[] targets,  int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.Kill,

            Targets = targets,

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

    public static TickInput NewBuild(int prefab, Vector2 position, int[] targets, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.NewBuild,

            Targets = targets,

            Prefab = prefab,

            Position = position,
            Performer = performer
        };
    }

    public static TickInput Destroy(int buildingID, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.Destroy,

            ID = buildingID,

            Performer = performer
        };
    }

    public static TickInput Build(int buildingID, int[] targets, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.Build,

            ID = buildingID,

            Targets = targets,

            Performer = performer
        };
    }

    public static TickInput Harvest(Vector2 position, int[] targets, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.Harvest,

            Targets = targets,
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
    public static TickInput Attack(int targetID, Vector2 targetpos, int[] attackers, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.Attack,

            Targets = attackers,

            ID = targetID,

            Position = targetpos,

            Performer = performer,
        };
    }

    public static TickInput GuardPosition(Vector2 position, int[] attackers, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.GuardPosition,

            Targets = attackers,

            Position = position,

            Performer = performer,
        };
    }
}
