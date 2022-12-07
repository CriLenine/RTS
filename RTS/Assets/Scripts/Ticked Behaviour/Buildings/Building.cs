using UnityEngine;

public abstract class Building : TickedBehaviour, IDamageable
{
    public enum Type
    {
        Farm,
        Barracks,
        ResourcesOutpost
    }

    [SerializeField]
    private bool _isBuilt = false;

    [SerializeField]
    protected BuildingData _data; //USE ?

    [SerializeField]
    private int _currentWorkforce;

    [SerializeField]
    private int _currentHealth;

    private Type _type;
    public Type BuildingType => _type;

    /*Ici on aura les options disponibles en cliquant sur un b�timent
     * (ex cr�er une certaine unit� dans une caserne)*/
    //[SerializeField]
    //private List<Option> _options;

    public BuildingData Data => _data;

    protected int MaxHealth => _data.MaxHealth;
    public float CurrentWorkforceRatio => _currentWorkforce / _data.TotalWorkforce;


    //SpriteManagement
    private SpriteRenderer _buildingRenderer;
    private int _actualSpriteIndex;
    private float _ratioStep;
    //

    private void Awake()
    {
        _buildingRenderer = GetComponent<SpriteRenderer>();

        _currentHealth = MaxHealth;
        _ratioStep = _data.TotalWorkforce / (_data.ConstructionSteps.Length);
        _actualSpriteIndex = 0;
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