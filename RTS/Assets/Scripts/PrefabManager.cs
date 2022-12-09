using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PrefabManager : MonoBehaviour
{
    private static PrefabManager _instance;

    [SerializeField]
    private List<CharacterData> _dataCharacters;

    [SerializeField]
    private List<BuildingData> _dataBuildings;

    public static List<BuildingData> DataBuildings => _instance._dataBuildings;
    public static List<CharacterData> DataCharacters => _instance._dataCharacters;

    private Dictionary<Building.Type, BuildingData> _buildingsPrefabs;
    private Dictionary<Character.Type, CharacterData> _charactersPrefabs;

    private void Awake()
    {
        if (_instance != null) return;

        _instance = this;
    }
    private void Start()
    {
        _buildingsPrefabs = new Dictionary<Building.Type, BuildingData>();
        _charactersPrefabs = new Dictionary<Character.Type, CharacterData>();

        foreach (var elem in _dataBuildings)
        {
            _buildingsPrefabs.Add(elem.Type, elem);
        }

        foreach (var elem in _dataCharacters)
        {
            _charactersPrefabs.Add(elem.Type, elem);
        }
    }

    public static BuildingData GetBuildingData(Building.Type peonBuilds)
    {
        return _instance._buildingsPrefabs[peonBuilds];
    }

    public static CharacterData GetCharacterData(Character.Type characterType)
    {
        return _instance._charactersPrefabs[characterType];
    }
}
