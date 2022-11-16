using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnableBuilding", menuName = "Spawnable/Building", order = 1)]
public class SpawnableDataBuilding : ScriptableObject
{
    [SerializeField]
    private PeonBuilds _type;

    [SerializeField]
    private Blueprint _buildingBlueprint;

    [SerializeField]
    private Building _building;

    [SerializeField]
    private int _neededPlayerLevel;

    [SerializeField]
    private GameManager.RessourceCost[] _cost;


    public PeonBuilds Type => _type;
    public Blueprint BuildingBlueprint => _buildingBlueprint;
    public Building Building => _building;
}
