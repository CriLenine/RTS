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

    /*Ici on aura les options disponibles en cliquant sur un b�timent
     * (ex cr�er une certaine unit� dans une caserne)*/
    //[SerializeField]
    //private List<Option> _options;

    public BuildingData Data => _buildingData;

    public int MaxHealth => _buildingData.MaxHealth;
    public float CurrentWorkforceRatio => _currentWorkforce / _buildingData.TotalWorkforce;
    public float CurrentHealth => _currentHealth / MaxHealth;

    //SpriteManagement
    private SpriteRenderer _buildingRenderer;
    private int _actualSpriteIndex;
    private float _ratioStep;
    //

    private void Start()
    {
        _buildingRenderer = GetComponent<SpriteRenderer>();

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
}