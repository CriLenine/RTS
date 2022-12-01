using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ressource", menuName = "Ressource", order = 1)]
public class RessourceData : ScriptableObject
{
    [SerializeField]
    private RessourceType _type;

    [SerializeField]
    private int _amount;

    [SerializeField]
    private float _harvestingTime;

    public RessourceType Type => _type;

    public int Amount { get => _amount; set { _amount = value; } }

    public float HarvestingTime => _harvestingTime;
}
