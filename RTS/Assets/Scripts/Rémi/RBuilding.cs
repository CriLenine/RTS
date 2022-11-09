using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RBuilding : TickedBehaviour, IDamageable
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
    public void AddWorkforce(int amount)
    {
        if (_isBuilt)
            Debug.LogError("Already Built !");

        _currentWorkforce += amount;
        if (CurrentWorkforce >= 1f)
        {
            _currentWorkforce = _buildingData.TotalWorkforce;
            _isBuilt = true;
        }
    }
}