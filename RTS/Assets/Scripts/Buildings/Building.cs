using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Building : TickedBehaviour, IDamageable
{
    [SerializeField]
    private bool _isBuilt = false;

    [SerializeField]
    private BuildingData _buildingData;

    [SerializeField]
    private int _currentWorkforce;

    [SerializeField]
    private int _currentHealth;

    /*Ici on aura les options disponibles en cliquant sur un bâtiment
     * (ex créer une certaine unité dans une caserne)*/
    //[SerializeField]
    //private List<Option> _options;

    public BuildingData Data => _buildingData;

    public int MaxHealth => _buildingData.MaxHealth;

    public float CurrentWorkforce => _currentWorkforce / _buildingData.TotalWorkforce;

    public float CurrentHealth => _currentHealth / MaxHealth;

    /// <returns><see langword="true"/> if it finishes the building's construction,
    /// <see langword="false"/> otherwise </returns>
    public bool AddWorkforce(int amount)
    {
        if (_isBuilt)
            return true;

        _currentWorkforce += amount;
        if (CurrentWorkforce >= 1f)
        {
            _currentWorkforce = _buildingData.TotalWorkforce;
            _isBuilt = true;
        }
        return _isBuilt;
    }
}