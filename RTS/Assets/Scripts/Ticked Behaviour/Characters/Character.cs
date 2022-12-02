using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public abstract class Character : TickedBehaviour, IDamageable
{
    public Action CurrentAction { get; private set; }

    private Queue<Action> _actions = new Queue<Action>();

    public enum Type
    {
        Peon,
        Knight
    }

    private Type _type;
    public Type CharaType => _type;

    [SerializeField]
    protected CharacterData _data;

    [SerializeField]
    private int _currentHealth;

    public abstract bool Idle { get; }
    public CharacterData Data => _data;

    protected int MaxHealth => Data.MaxHealth;
    

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
        _currentHealth = MaxHealth;
    }

    protected virtual void Update()
    {
        if (CurrentAction is Move)
        {
            Move move = CurrentAction as Move;

            _pathRenderer.positionCount = move.Positions.Count - move.Index + 1;

            int j = 0,  i;

            _pathRenderer.SetPosition(j++, transform.position);

            for (i = move.Index; i < move.Positions.Count; ++i, ++j)
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
        if (CurrentAction is null)
            CheckSurrounding();

        else if (CurrentAction.Perform() == true)
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

    private void CheckSurrounding()
    {
        List<Character> charas = GameManager.Characters.ToList();
        float attackRange = Data.AutoAttackDistance;

        for (int i = 0; i < charas.Count; i++) //On regarde tous les charas du jeux
        {
            Character chara = charas[i];
            if (chara.Performer == Performer || (chara.transform.position - transform.position).sqrMagnitude >= attackRange) continue; //Si trop loin ou meme team => next

            //Sinon on renvois l'action d'attaque
            SetAction(new Attack(this, chara, false));
            return;
        }
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

    public bool TakeDamage(int damage)
    {
        return (_currentHealth -= damage) <= 0;
    }

    public void GainHealth(int amount)
    {
        if ((_currentHealth += amount) > MaxHealth)
            _currentHealth = MaxHealth;
    }

    protected void SetType(Type type)
    {
        _type = type;
    }
}