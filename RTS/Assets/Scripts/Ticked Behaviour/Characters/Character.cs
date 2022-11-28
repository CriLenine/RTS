using System.Collections.Generic;
using UnityEngine;

public abstract class Character : TickedBehaviour, IDamageable
{
    public Action CurrentAction { get; private set; }

    private Queue<Action> _actions = new Queue<Action>();

    public enum Type
    {
        Peon,
        Knight
    }
    protected Type _type;
    public Type CharaType;

    [SerializeField]
    protected CharacterData _data;

    public abstract bool Idle { get; }
    public CharacterData Data => _data;

    public int MaxHealth => Data.MaxHealth;

    public GameObject SelectionMarker;
    public Vector2Int Coords;

    public Stack<LogicalTile> Path;

    protected virtual void Awake()
    {
        Coords = TileMapManager.WorldToTilemapCoords(gameObject.transform.position);
    }

    protected virtual void Start()
    {

    }

    public sealed override void Tick()
    {
        if (CurrentAction?.Perform() == true)
            CurrentAction = _actions.Count > 0 ? _actions.Dequeue() : null;

        Coords = TileMapManager.WorldToTilemapCoords(gameObject.transform.position);
    }

    public void AddAction(Action action)
    {
        _actions.Enqueue(action);

        CurrentAction ??= _actions.Dequeue();
    }

    public void SetAction(Action action)
    {
        CurrentAction = null;

        _actions.Clear();

        AddAction(action);
    }

    public void DebugCoordinates()
    {
        Debug.Log($"{gameObject.name} coords : ({Coords.x}, {Coords.y})");
    }

    public override Hash128 GetHash128()
    {
        Hash128 hash = base.GetHash128();

        hash.Append(Coords.x);
        hash.Append(Coords.y);

        return hash;
    }
}