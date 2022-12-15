using UnityEngine;

public abstract class Building : TickedBehaviour, IDamageable
{
    public enum Type
    {
        HeadQuarters,
        Housing,
        Sawmill,
        Quarry,
        Barracks
    }


    [Header("Base Data")]
    [Space]

    [SerializeField]
    protected BuildingData _buildingData;

    [SerializeField]
    private int _currentHealth;
    public int CurrentHealth => _currentHealth;

    [SerializeField]
    protected HealthBar HealthBar;

    [Space]
    [Space]

    [Header("Utils")]
    [Space]

    [SerializeField]
    private BoxCollider2D _boxCollider;

    [SerializeField]
    protected LineRenderer _pathRenderer;
    public LineRenderer PathRenderer => _pathRenderer;

    [Space]
    [Space]

    [Header("Construction Progression Art")]
    [Space]

    [Header("Transforms")]
    [SerializeField]
    private Transform _visualTransform;
    [SerializeField]
    private Transform _completionVisualTransform, _healthBarTransform;

    [Header("SpriteRenderers")]
    [SerializeField]
    private SpriteRenderer _completionVisualSprite;
    [SerializeField]
    private SpriteRenderer _iconSprite, _visualBackgroundSprite;

    [Header("Colors")]
    [SerializeField]
    private Color _completionVisualStartColor;
    [SerializeField]
    private Color _completionVisualEndColor, _iconSpriteStartColor, _iconSpriteEndColor, _selectedColor;
    [SerializeField]
    private Color _visualBackgroundStartColor, _visualBackgroundEndColor;


    private bool _buildComplete;
    public bool BuildComplete => _buildComplete;

    private int _completedBuildTicks;


    protected Type _type;
    public Type BuildingType => _type;
    public BuildingData Data => _buildingData;
    protected int MaxHealth => _buildingData.MaxHealth;
    public float BuildCompletionRatio => (float)_completedBuildTicks / _buildingData.RequiredBuildTicks;

    private bool _selected;

    protected override void Awake()
    {
        base.Awake();

        _type = Data.Type;

        float scale = .95f * (TileMapManager.TileSize * (1 + 2 * Data.Outline));
        _visualTransform.localScale = new Vector3(scale, scale, 1);
        _boxCollider.size = new Vector2(scale, scale);

        _healthBarTransform.transform.position += new Vector3(0, .5f * scale);

        _currentHealth = MaxHealth;
        HealthBar.SetHealth(1);

        _completedBuildTicks = 0;
        _buildComplete = false;

        _completionVisualTransform.localScale = new Vector3(0, 0, 1);

        _completionVisualSprite.color = _completionVisualStartColor;
        _iconSprite.sprite = Data.HUDIcon;
        _iconSprite.color = _iconSpriteStartColor;
        _visualBackgroundSprite.color = _visualBackgroundStartColor;
    }

    private void Update()
    {
        if (_selected && this is ISpawner spawner)
        {
            Vector2 rallypoint = spawner.GetRallyPoint();

            _pathRenderer.SetPosition(0, transform.position);
            _pathRenderer.SetPosition(1, rallypoint);

            _pathRenderer.transform.position = rallypoint;

            _pathRenderer.startColor = Color.cyan;
            _pathRenderer.endColor = Color.cyan;

            _pathRenderer.gameObject.SetActive(true);
        }
    }

    /// <returns><see langword="true"/> if it finishes the building's construction,
    /// <see langword="false"/> otherwise </returns>
    public bool CompleteBuild(int completionAmount)
    {
        if (_buildComplete)
            return true;

        _completedBuildTicks += completionAmount;

        if (BuildCompletionRatio >= 1f)
        {
            // REQUIRES FIXING POSITION WHEN BUILD ORDER IS SENT !
            // TileMapManager.AddBuilding(Data.Outline, transform.position);

            _completedBuildTicks = _buildingData.RequiredBuildTicks;
            _iconSprite.color = _iconSpriteEndColor;

            if(!_selected)
                _visualBackgroundSprite.color = _visualBackgroundEndColor;

            if (GameManager.MyBuildings.Contains(ID))
                GameManager.UpdateHousing(Data.HousingProvided);

            _buildComplete = true;
        }

        float completionValue = Mathf.Lerp(.5f, .98f, BuildCompletionRatio);
        _completionVisualTransform.localScale = new Vector3(completionValue, completionValue, 1);

        _completionVisualSprite.color = Color.Lerp(_completionVisualStartColor, _completionVisualEndColor, BuildCompletionRatio);

        return _buildComplete;
    }

    public void Select()
    {
        _selected = true;

        if (_buildComplete)
            _visualBackgroundSprite.color = _selectedColor;
    }

    public void Unselect()
    {
        _selected = false;
        _visualBackgroundSprite.color = _buildComplete ? _visualBackgroundEndColor : _visualBackgroundStartColor;

        if(this is ISpawner)
            _pathRenderer.gameObject.SetActive(false);
    }

    public bool TakeDamage(int damage)
    {
        if (_currentHealth == MaxHealth)
            HealthBar.gameObject.SetActive(true);

        _currentHealth -= damage;
        HealthBar.SetHealth((float)_currentHealth / MaxHealth); 
        return _currentHealth <= 0;
    }

    public void GainHealth(int amount)
    {
        _currentHealth += amount;

        HealthBar.SetHealth((float)_currentHealth / MaxHealth);

        if (_currentHealth >= MaxHealth)
            HealthBar.gameObject.SetActive(false);

        if (_currentHealth > MaxHealth)
            _currentHealth = MaxHealth;
    }
}