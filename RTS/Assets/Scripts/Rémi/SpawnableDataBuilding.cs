using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnableBuilding", menuName = "Spawnable/Building", order = 1)]
public class SpawnableDataBuilding : ScriptableObject
{
    [SerializeField]
    private RBuilding _building;

    [SerializeField]
    private int _neededPlayerLevel;

    [SerializeField]
    private GameManager.RessourceCost[] _cost;
}
