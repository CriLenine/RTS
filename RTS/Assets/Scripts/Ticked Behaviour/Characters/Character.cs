using UnityEngine;
using System.Collections.Generic;

public abstract class Character : TickedBehaviour, IDamageable
{
    private Action _currentAction;

    private Queue<Action> _actions;

    public enum Type
    {
        Peon,
        Knight
    }

    [SerializeField]
    protected CharacterData _data;

    public abstract bool Idle { get; }
    public CharacterData Data => _data;

    public int MaxHealth => _data.MaxHealth;

    public GameObject SelectionMarker;
    public Vector2Int Coords;

    public Stack<LogicalTile> Path;

    protected virtual void Start()
    {
        _actions = new Queue<Action>();

        Coords = TileMapManager.WorldToTilemapCoords(gameObject.transform.position);
    }

    private void Update()
    {
        Coords = TileMapManager.WorldToTilemapCoords(gameObject.transform.position);
    }

    public sealed override void Tick()
    {
        if (_currentAction?.Perform() == true)
            _currentAction = _actions.Count > 0 ? _actions.Dequeue() : null;
    }

    public void AddAction(Action action)
    {
        _actions.Enqueue(action);

        _currentAction ??= _actions.Dequeue();
    }

    public void SetAction(Action action)
    {
        _currentAction = null;

        _actions.Clear();

        AddAction(action);
    }

    public void DebugCoordinates()
    {
        Debug.Log($"{gameObject.name} coords : ({Coords.x}, {Coords.y})");
    }
}