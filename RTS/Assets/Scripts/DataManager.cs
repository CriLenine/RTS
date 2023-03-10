using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class DataManager : MonoBehaviour
{
    private static DataManager _instance;

    [Separator("Prefabs")]

    [SerializeField]
    private Character _character;
    public static Character CharacterPrefab => _instance._character;

    [SerializeField]
    private Building _building;
    public static Building BuildingPrefab => _instance._building;

    [Space]
    [Separator("Data")]

    [SerializeField]
    private List<CharacterData> _dataCharacters;

    [SerializeField]
    private List<BuildingData> _dataBuildings;

    private Dictionary<Building.Type, BuildingData> _buildingsData;
    private Dictionary<Character.Type, CharacterData> _charactersData;

    private void Awake()
    {
        if (_instance != null) return;

        _instance = this;
    }
    private void Start()
    {
        _buildingsData = new Dictionary<Building.Type, BuildingData>();
        _charactersData = new Dictionary<Character.Type, CharacterData>();

        foreach (BuildingData data in _dataBuildings)
            _buildingsData.Add(data.Type, data);

        foreach (CharacterData data in _dataCharacters)
            _charactersData.Add(data.Type, data);
    }

    public static BuildingData GetBuildingData(Building.Type buildingType)
    {
        return _instance._buildingsData[buildingType];
    }

    public static CharacterData GetCharacterData(Character.Type characterType)
    {
        return _instance._charactersData[characterType];
    }
}
