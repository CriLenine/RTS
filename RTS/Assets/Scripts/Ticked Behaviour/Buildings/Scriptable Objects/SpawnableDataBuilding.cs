using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnableBuilding", menuName = "Spawnable/Building", order = 1)]
public class SpawnableDataBuilding : ScriptableObject
{
    [SerializeField]
    private Building.Type _type;

    [Header("Building")]
    [SerializeField]
    private Blueprint _buildingBlueprint;

    [SerializeField]
    private Building _building;

    [Header("Data")]
    [SerializeField]
    private int _neededPlayerLevel;

    [SerializeField]
    private GameManager.RessourceCost[] _cost;

    [Header("UI")]
    [SerializeField]
    private Sprite _buildingUiIcon;
    public Building.Type Type => _type;
    public Blueprint BuildingBlueprint => _buildingBlueprint;
    public Building Building => _building;

    public Sprite BuildingUiIcon => _buildingUiIcon;
}
