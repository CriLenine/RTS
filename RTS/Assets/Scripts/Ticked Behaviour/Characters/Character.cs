using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyBox;

public class Character : TickedBehaviour, IDamageable
{
    public enum Type
    {
        Peon,
        Naked,
        Bowman,
        Paladin,
        All
    }

    public Action CurrentAction { get; private set; }
    private Queue<Action> _actions = new Queue<Action>();

    private TickedBehaviour _currentTarget = null;

    [Separator("Base Data")]
    [Space]

    [ReadOnly]
    [SerializeField]
    private int _currentHealth;
    public int CurrentHealth => _currentHealth;

    protected CharacterData _data;
    public CharacterData Data => _data;

    [SerializeField]
    private Animator _animator;
    public Animator Animator => _animator;

    [SerializeField]
    protected HealthBar HealthBar;

    [SerializeField]
    private LineRenderer _pathRenderer;

    [SerializeField]
    private GameObject HoverMarker;

    [SerializeField]
    private GameObject SelectionMarker; 

    [Separator("UI")]
    [Space]

    [SerializeField]
    private SpriteRenderer _iconSprite;

    public Resource.Amount HarvestedResource;

    private bool _isSelected = false;
    public bool Idle => _actions.Count == 0;

    private bool _isAgressed = false;
    private bool _isGuardingPosition = false;

    private bool _isShooting = false;
    private GameObject _projectile;
    private float _projectileSpeed;
    public override void InitData<T>(T data) 
    {
        _data = data as CharacterData;


        _animator.runtimeAnimatorController = _data.AnimatorCtrller;
        _iconSprite.sprite = _data.CharacterSprite;
        _currentHealth = _data.MaxHealth;
        HealthBar.SetHealth(1);
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected virtual void LateUpdate()
    {
        if (CurrentAction?.SpecificAction is Move move && move.Positions?.Count > 0)
        {
            _pathRenderer.positionCount = move.Positions.Count - move.Index + 1;

            int j = 0,  i;

            _pathRenderer.SetPosition(j++, transform.position);

            for (i = move.Index; i < move.Positions.Count; ++i, ++j)
                _pathRenderer.SetPosition(j, move.Positions[i]);

            _pathRenderer.transform.position = move.Positions[^1];

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
            if (_isGuardingPosition)
                CheckSurrounding();
            else
                _isAgressed = _isAgressed && CheckSurrounding();
        else if (CurrentAction.Perform())
            CurrentAction = _actions.Count > 0 ? _actions.Dequeue() : null;

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if(Data.Type == Type.Peon)
        {
            if (CurrentAction is Build)
                _animator.Play("Build");
            else if (CurrentAction is Harvest harvest)
                _animator.Play(harvest.GetHarvestAnimationName());
        }

        if (CurrentAction is Move)
            _animator.Play("Walk");
        else if (_currentHealth > 0 && CurrentAction is null)
            _animator.Play("Idle");

        //shootingAnimation
        if (_isShooting)
        {
            if (_currentTarget == null || (_currentTarget.transform.position-_projectile.transform.position).magnitude < 0.1f)
            {
                if (_projectile != null)
                    Destroy(_projectile);

                _isShooting = false;
                return;
            }

            _projectile.transform.right = _currentTarget.transform.position - _projectile.transform.position;
            Vector2 movement = Vector2.MoveTowards(_projectile.transform.position, _currentTarget.transform.position, _projectileSpeed * 0.1f);

            _projectile.transform.position = movement;
        }
    }

    public void Select()
    {
        _isSelected = true;

        HoverMarker.SetActive(false);
        SelectionMarker.SetActive(true);
        HealthBar.gameObject.SetActive(true);
    }

    public void Unselect()
    {
        _isSelected = false;

        SelectionMarker.SetActive(false);

        if(CurrentHealth == Data.MaxHealth)
            HealthBar.gameObject.SetActive(false);
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
        _isGuardingPosition = false; //stop watching if action added

        _actions.Enqueue(action);

        CurrentAction ??= _actions.Dequeue();
    }

    public void ClearActions()
    {
        CurrentAction = null;

        _actions.Clear();
    }

    public void SetAction(Action action)
    {
        ClearActions();

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
        if (_currentHealth == Data.MaxHealth)
            HealthBar.gameObject.SetActive(true);
        _isAgressed = true;

        GameEventsManager.PlayEvent("TakeDamage", transform.position);

        _currentHealth -= damage;
        HealthBar.SetHealth((float)_currentHealth/ Data.MaxHealth);
        return _currentHealth <= 0;
    }

    public void GainHealth(int amount)
    {
        _currentHealth += amount;


        HealthBar.SetHealth(_currentHealth);

        if (_currentHealth >= Data.MaxHealth)
            HealthBar.gameObject.SetActive(false);

        if (_currentHealth > Data.MaxHealth)
            _currentHealth = Data.MaxHealth;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;

        Position = position;
        Coords = TileMapManager.WorldToTilemapCoords(position);
    }

    public void SetResource(Resource.Amount harvestedResource)
    {
        HarvestedResource = harvestedResource;
    }

    public void SetTarget(TickedBehaviour target) => _currentTarget = target;

    public void BeginWatch() => _isWatching = true;
    public void Shoot(GameObject projectile, float projectileSpeed)
    {
        _isShooting = true;
        _projectile = projectile;
        _projectileSpeed = projectileSpeed;
    }


    private void OnMouseOver()
    {
        if (!_isSelected && Performer == NetworkManager.Me)
            HoverMarker.SetActive(true);
    }

    private void OnMouseExit()
    {
        if (!_isSelected && Performer == NetworkManager.Me)
            HoverMarker.SetActive(false);
    }
}