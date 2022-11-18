using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PrefabManager : MonoBehaviour
{
    private static PrefabManager _instance;

    [SerializeField]
    private List<SpawnableDataCharacter> _dataCharacters;

    [SerializeField]
    private List<SpawnableDataBuilding> _dataBuildings;

    public static List<SpawnableDataBuilding> DataBuildings => _instance._dataBuildings;

    private Dictionary<Building.Type, SpawnableDataBuilding> _buildingsPrefabs;
    private Dictionary<Character.Type, SpawnableDataCharacter> _charactersPrefabs;

    private void Awake()
    {
        if (_instance != null) return;

        _instance = this;
    }
    private void Start()
    {
        _buildingsPrefabs = new Dictionary<Building.Type, SpawnableDataBuilding>();
        _charactersPrefabs = new Dictionary<Character.Type, SpawnableDataCharacter>();

        foreach (var elem in _dataBuildings)
        {
            _buildingsPrefabs.Add(elem.Type, elem);
        }

        foreach (var elem in _dataCharacters)
        {
            _charactersPrefabs.Add(elem.Type, elem);
        }
    }

    public static SpawnableDataBuilding GetBuildingData(Building.Type peonBuilds)
    {
        return _instance._buildingsPrefabs[peonBuilds];
    }

    public static SpawnableDataBuilding GetBuildingData(int peonBuilds)
    {
        return GetBuildingData((Building.Type)peonBuilds);
    }

    public static SpawnableDataCharacter GetCharacterData(Character.Type characterType)
    {
        return _instance._charactersPrefabs[characterType];
    }

    public static SpawnableDataCharacter GetCharacterData(int characterType)
    {
        return GetCharacterData((Character.Type)characterType);
    }
}
