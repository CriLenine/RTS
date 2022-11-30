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

    private LineRenderer _pathRenderer;

    protected virtual void Awake()
    {
        _pathRenderer = GetComponentInChildren<LineRenderer>(true);

        Coords = TileMapManager.WorldToTilemapCoords(gameObject.transform.position);
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        if (CurrentAction is Move)
        {
            Move move = CurrentAction as Move;

            _pathRenderer.positionCount = move.Positions.Length - move.Index + 1;

            int j = 0,  i;

            _pathRenderer.SetPosition(j++, transform.position);

            for (i = move.Index; i < move.Positions.Length; ++i, ++j)
                _pathRenderer.SetPosition(j, move.Positions[i]);

            _pathRenderer.transform.position = move.Positions[i - 1];

            _pathRenderer.startColor = GameManager.Colors[Performer];
            _pathRenderer.endColor = GameManager.Colors[Performer];

            _pathRenderer.gameObject.SetActive(true);
        }
        else
            _pathRenderer.gameObject.SetActive(false);
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