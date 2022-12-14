using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public enum ResourceType2
{
    crystal,
    wood,
    gold,
    stone
}

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

    private UIInputs _uiInputs;

    protected void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this);

        _instance._resources.Show();
        _instance._population.Show();

        _uiInputs = new UIInputs();
        _uiInputs.Enable();
    }

    public static void DisplayStats(Character character)
    {
        _instance._stats.DisplayStats(character);
    }

    public static void UpdateHUD(List<Character> characters)
    {
        _instance._stats.Hide();
        _instance._actions.Hide();
        _instance._buildings.Hide();

        int count = characters.Count;

        if (count == 0)
            return;

        _instance._actions.Show();

        if(count == 1)
        {
            _instance._stats.DisplayStats(characters[0]);
            if (characters[0] is Peon)
                _instance._buildings.Show();
            return;
        }

        _instance._buildings.Show();

        for (int i = 0; i < count; ++i)
            if(characters[i] is not Peon)
            {
                _instance._buildings.Hide();
                return;
            }
    }

    public static UIInputs GetUIInputs() => _instance._uiInputs;
}
