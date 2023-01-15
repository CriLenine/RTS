using UnityEngine;

public enum InputType
{
    QueueSpawn,
    UnqueueSpawn,
    Move,
    NewBuild,
    Build,
    Harvest,
    Hunt,
    Attack,
    GameOver,
    Stop,
    Kill,
    GuardPosition,
    Destroy,
    CancelConstruction,
    UpdateRallyPoint
    Deposit
}

public class TickInput
{
    public int Performer;

    public InputType Type;

    public int[] Targets;

    public int ID = -1;
    public int Prefab;

    public Vector2 Position;

    public static TickInput QueueSpawn(int prefab, int spawnerID, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.QueueSpawn,

            ID = spawnerID,
            Prefab = prefab,

            Performer = performer
        };
    }
    public static TickInput UnqueueSpawn(int prefab, int spawnerID, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.UnqueueSpawn,

            ID = spawnerID,
            Prefab = prefab,

            Performer = performer
        };
    }
    public static TickInput UpdateRallyPoint(int spawnerID,Vector2 newRallyPoint, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.UpdateRallyPoint,

            ID = spawnerID,
            Position = newRallyPoint,

            Performer = performer
        };
    }
    public static TickInput Stop(int[] targets, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.Stop,

            Targets = targets,

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

    public static TickInput CancelConstruction(int buildingID, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.CancelConstruction,

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
    
    public static TickInput Deposit(int buildingID, int[] targets, int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.Deposit,

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

    public static TickInput GameOver(int performer = 0)
    {
        return new TickInput()
        {
            Type = InputType.GameOver,

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
