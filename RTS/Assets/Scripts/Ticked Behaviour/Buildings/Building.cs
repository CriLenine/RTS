using UnityEngine;

public abstract class Building : TickedBehaviour, IDamageable
{
    public enum Type
    {
        Farm,
        Barracks,
        PlutoniumOutpost,
        GoldOutpost
    }

    [SerializeField]
    private bool _isBuilt = false;

    [SerializeField]
    private BuildingData _buildingData;

    [SerializeField]
    private int _currentWorkforce;

    [SerializeField]
    private int _currentHealth;

    [SerializeField]
    protected HealthBar HealthBar;

    private Type _type;
    public Type BuildingType => _type;

    /*Ici on aura les options disponibles en cliquant sur un b�timent
     * (ex cr�er une certaine unit� dans une caserne)*/
    //[SerializeField]
    //private List<Option> _options;

    public BuildingData Data => _data;

    protected int MaxHealth => _data.MaxHealth;
    public float CurrentWorkforceRatio => _currentWorkforce / _data.TotalWorkforce;

    private LineRenderer _pathRenderer;
    public LineRenderer PathRenderer => _pathRenderer;

    //SpriteManagement
    private SpriteRenderer _buildingRenderer;
    private int _actualSpriteIndex;
    private float _ratioStep;
    //

    private void Awake()
    {
        _buildingRenderer = GetComponent<SpriteRenderer>();
        _pathRenderer = GetComponentInChildren<LineRenderer>(true);

        _currentHealth = MaxHealth;
        HealthBar.SetMaxHealth(MaxHealth);
        _ratioStep = _buildingData.TotalWorkforce / (_buildingData.ConstructionSteps.Length);
        _actualSpriteIndex = 0;
    }

    private void Update()
    {
        if (UIManager.CurrentManager is BuildingUI buildingUI)
        {
            if (!buildingUI.Building.TryGetComponent(out ISpawner spawner)) return;

            Vector2 rallypoint = spawner.GetRallyPoint();
            _pathRenderer.SetPosition(0, transform.position);
            _pathRenderer.SetPosition(1, rallypoint);

            _pathRenderer.transform.position = rallypoint;

            _pathRenderer.startColor = Color.cyan;
            _pathRenderer.endColor = Color.cyan;

            _pathRenderer.gameObject.SetActive(true);
        }
        else
            _pathRenderer.gameObject.SetActive(false);
    }
    /// <returns><see langword="true"/> if it finishes the building's construction,
    /// <see langword="false"/> otherwise </returns>
    public bool AddWorkforce(int amount)
    {
        if (_isBuilt)
            return true;

        _currentWorkforce += amount;

        //Change sprite 
        int spriteIndex=0;
        
        for (int i = 0; i < _data.ConstructionSteps.Length; i++)
        {
            spriteIndex = _currentWorkforce > (i * (_ratioStep)) ? i:spriteIndex ;
        }

        if(spriteIndex != _actualSpriteIndex)
        {
            _buildingRenderer.sprite = _data.ConstructionSteps[spriteIndex];
            _actualSpriteIndex = spriteIndex;
        }

        //

        if (CurrentWorkforceRatio >= 1f)
        {
            _currentWorkforce = _data.TotalWorkforce;
            _isBuilt = true;
        }
        return _isBuilt;
    }

    public bool TakeDamage(int damage)
    {
        if (_currentHealth == MaxHealth)
            HealthBar.gameObject.SetActive(true);

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