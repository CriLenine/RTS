using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Character : TickedBehaviour, IDamageable
{
    public Action CurrentAction { get; private set; }

    private Queue<Action> _actions = new Queue<Action>();

    public enum Type
    {
        None,
        Peon,
        Naked
    }

    private Type _type;
    public Type CharaType => _type;

    [SerializeField]
    protected HealthBar HealthBar;

    [SerializeField]
    protected CharacterData _data;

    [SerializeField]
    private int _currentHealth;


    public int CurrentHealth => _currentHealth;

    public abstract bool Idle { get; }
    public CharacterData Data => _data;

    protected int MaxHealth => Data.MaxHealth;
    

    public GameObject SelectionMarker;

    private LineRenderer _pathRenderer;
    private bool _isAgressed = false;

    protected virtual void Awake()
    {
        _pathRenderer = GetComponentInChildren<LineRenderer>(true);
    }

    protected virtual void Start()
    {
        _currentHealth = MaxHealth;
        HealthBar.SetMaxHealth(MaxHealth);
    }

    protected virtual void Update()
    {
        if (CurrentAction?.SpecificAction is Move move)
        {
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
            _isAgressed = _isAgressed && CheckSurrounding();

        else if (CurrentAction.Perform())
        {
            CurrentAction = _actions.Count > 0 ? _actions.Dequeue() : null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        int radius = 10;
        int squareRadius = radius * radius;

        Color grey = Color.grey;
        grey.a = 0.1f;

        Gizmos.color = grey;

        for (int i = Coords.x - 10, j; i <= Coords.x + 10; ++i)
            for (j = Coords.y - 10; j <= Coords.y + 10; ++j)
                if ((i - Coords.x) * (i - Coords.x) + (j - Coords.y) * (j - Coords.y) < squareRadius)
                    Gizmos.DrawWireCube(TileMapManager.TilemapCoordsToWorld(new Vector2Int(i, j)), Vector3.one * TileMapManager.TileSize);
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

    private bool CheckSurrounding()
    {
        List<Character> charas = GameManager.Characters.ToList();
        float attackRange = Data.AttackRange;

        for (int i = 0; i < charas.Count; i++) //On regarde tous les charas du jeux
        {
            Character chara = charas[i];
            if (chara.Performer == Performer || (chara.transform.position - transform.position).sqrMagnitude >= attackRange) continue; //Si trop loin ou meme team => next

            //Sinon on renvois l'action d'attaque
            SetAction(new Attack(this, chara, false));
            return false;
        }

        return false;
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
        if (_currentHealth == MaxHealth)
            HealthBar.gameObject.SetActive(true);

        _isAgressed = true;

        _currentHealth -= damage;
        HealthBar.SetHealth(_currentHealth);
        return _currentHealth <= 0;
    }

    public void GainHealth(int amount)
    {
        _currentHealth += amount;


        HealthBar.SetHealth(_currentHealth);

        if (_currentHealth >= MaxHealth)
            HealthBar.gameObject.SetActive(false);

        if (_currentHealth > MaxHealth)
            _currentHealth = MaxHealth;
    }

    protected void SetType(Type type)
    {
        _type = type;
    }
}