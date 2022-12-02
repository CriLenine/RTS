using UnityEngine;

public abstract class Building : TickedBehaviour, IDamageable
{
    public enum Type
    {
        Farm,
        Barracks,
        Ressourcesoutpost
    }

    [SerializeField]
    private bool _isBuilt = false;

    [SerializeField]
    private BuildingData _buildingData; //USE ?

    [SerializeField]
    private int _currentWorkforce;

    [SerializeField]
    private int _currentHealth;

    private Type _type;
    public Type BuildingType => _type;

    /*Ici on aura les options disponibles en cliquant sur un bâtiment
     * (ex créer une certaine unité dans une caserne)*/
    //[SerializeField]
    //private List<Option> _options;

    public BuildingData Data => _buildingData;

    protected int MaxHealth => _buildingData.MaxHealth;
    public float CurrentWorkforceRatio => _currentWorkforce / _buildingData.TotalWorkforce;


    //SpriteManagement
    private SpriteRenderer _buildingRenderer;
    private int _actualSpriteIndex;
    private float _ratioStep;
    //

    private void Awake()
    {
        _buildingRenderer = GetComponent<SpriteRenderer>();

        _currentHealth = MaxHealth;
        _ratioStep = _buildingData.TotalWorkforce / (_buildingData.ConstructionSteps.Length);
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
        
        for (int i = 0; i < _buildingData.ConstructionSteps.Length; i++)
        {
            spriteIndex = _currentWorkforce > (i * (_ratioStep)) ? i:spriteIndex ;
        }

        if(spriteIndex != _actualSpriteIndex)
        {
            _buildingRenderer.sprite = _buildingData.ConstructionSteps[spriteIndex];
            _actualSpriteIndex = spriteIndex;
        }

        //

        if (CurrentWorkforceRatio >= 1f)
        {
            _currentWorkforce = _buildingData.TotalWorkforce;
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