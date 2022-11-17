using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Characters
{
    Peon,
    Knight
}
public class PrefabManager : MonoBehaviour
{
    private static PrefabManager _instance;

    [SerializeField]
    private List<SpawnableDataCharacter> _dataCharacters;

    [SerializeField]
    private List<SpawnableDataBuilding> _dataBuildings;

    private Dictionary<PeonBuilds, SpawnableDataBuilding> _buildingsPairs;
    private Dictionary<Characters, SpawnableDataCharacter> _charactersPairs;

    private void Awake()
    {
        if (_instance != null) return;

        _instance = this;
    }
    private void Start()
    {
        _buildingsPairs = new Dictionary<PeonBuilds, SpawnableDataBuilding>();
        _charactersPairs = new Dictionary<Characters, SpawnableDataCharacter>();

        foreach (var elem in _dataBuildings)
        {
            _buildingsPairs.Add(elem.Type, elem);
        }

        foreach (var elem in _dataCharacters)
        {
            _charactersPairs.Add(elem.Type, elem);
        }
    }

    public static List<SpawnableDataBuilding> GetBuildingDatas => _instance._dataBuildings;
    public static SpawnableDataBuilding GetBuildingData(PeonBuilds peonBuilds) => _instance._buildingsPairs[peonBuilds];
    public static SpawnableDataCharacter GetCharacterData(Characters character) => _instance._charactersPairs[character];
}
