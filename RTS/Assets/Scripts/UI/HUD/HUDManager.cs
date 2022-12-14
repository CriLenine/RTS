using UnityEngine;
using System.Collections.Generic;

public class HUDManager : MonoBehaviour
{
    private static HUDManager _instance;

    [Header("HUD Components")]

    [SerializeField]
    private HUDResources _resources;
    [SerializeField]
    private HUDPopulation _population;
    [SerializeField]
    private HUDStats _stats;
    [SerializeField]
    private HUDActions _actions;
    [SerializeField]
    private HUDBuildings _buildings;

    protected void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this);

        _instance._resources.Show();
        _instance._population.Show();
    }

    public static void DisplayStats(Character character)
    {
        _instance._stats.DisplayStats(character);
    }

    public static void UpdateHUD()
    {
        List<Character> characters = CharacterManager.SelectedCharacters;
        Building building = CharacterManager.SelectedBuilding;

        _instance._stats.Hide();
        _instance._actions.Hide();
        _instance._buildings.Hide();

        int charactersCount = characters.Count;

        if (charactersCount == 0 && building == null)
            return;

        _instance._actions.Show();

        if (building != null)
        {
            _instance._stats.DisplayStats(building);
            _instance._actions.UpdateActions(building.BuildingType);
            return;
        }

        _instance._actions.UpdateActions();
        
        if (charactersCount == 1)
        {
            _instance._stats.DisplayStats(characters[0]);
            if (characters[0] is Peon)
                _instance._buildings.Show();
            return;
        }

        _instance._buildings.Show();

        for (int i = 0; i < charactersCount; ++i)
            if(characters[i] is not Peon)
            {
                _instance._buildings.Hide();
                return;
            }
    }

    public static void UpdateHousing()
    {
        _instance._population.UpdateHousing();
    }
}
