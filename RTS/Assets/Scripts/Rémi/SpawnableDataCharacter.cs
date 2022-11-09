using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnableCharacter", menuName = "Spawnable/Character", order = 1)]
public class SpawnableDataCharacter : ScriptableObject
{
    [SerializeField]
    private RCharacter _character;

    [SerializeField]
    private int _neededBuildingLevel;

    [SerializeField]
    private GameManager.RessourceCost[] _cost;
}
