using System;
using UnityEngine;
using System.Collections.Generic;
using MyBox;

public class HUDManager : MonoBehaviour
{
    private static HUDManager _instance;

    [Serializable]
    struct ResourceSpecs
    {
        public ResourceType Type;
        public Sprite Sprite;
        public Color Color;
    }

    [Separator("HUD Components")]

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

    [Space]
    [Separator("Art")]

    [SerializeField]
    private GameObject _buttonFill;
    public static GameObject ButtonFill => _instance._buttonFill;

    [SerializeField]
    private Color _defaultButtonColor;
    public static Color DefaultButtonColor => _instance._defaultButtonColor;

    [Space]

    [SerializeField]
    private Sprite _economyTypeSprite;
    public static Sprite EconomyTypeSprite => _instance._economyTypeSprite;

    [SerializeField]
    private Sprite _militaryTypeSprite;
    public static Sprite MilitaryTypeSprite => _instance._militaryTypeSprite;

    [SerializeField]
    private Color _economyTypeColor;
    public static Color EconomyTypeColor => _instance._economyTypeColor;

    [SerializeField]
    private Color _militaryTypeColor;
    public static Color MilitaryTypeColor => _instance._militaryTypeColor;

    [Space]
    [Space]

    [SerializeField]
    private List<ResourceSpecs> _resourcesSpecs;

    private Dictionary<ResourceType, Sprite> _resourceSprites = new Dictionary<ResourceType, Sprite>();
    public static Dictionary<ResourceType, Sprite> ResourceSprites => _instance._resourceSprites;

    private Dictionary<ResourceType, Color> _resourceColors = new Dictionary<ResourceType, Color>();
    public static Dictionary<ResourceType, Color> ResourceColors => _instance._resourceColors;

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

        foreach(ResourceSpecs spec in _resourcesSpecs)
        {
            _resourceSprites.Add(spec.Type, spec.Sprite);
            _resourceColors.Add(spec.Type, spec.Color);
        }
    }

    public static void DisplayStats(Character character)
    {
        _instance._stats.DisplayStats(character);
    }

    public static void UpdateHUD()
    {
        ToolTipManager.HideToolTip();

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
            _instance._actions.ShowBuildingActions(building.Data);
            return;
        }

        _instance._actions.ShowCharacterActions();
        
        if (charactersCount == 1)
        {
            _instance._stats.DisplayStats(characters[0]);
            if (characters[0].Data.CanBuild)
                _instance._buildings.Show();
            return;
        }

        _instance._buildings.Show();

        for (int i = 0; i < charactersCount; ++i)
            if(!characters[i].Data.CanBuild)
            {
                _instance._buildings.Hide();
                return;
            }
    }

    public static UIInputs GetUIInputs() => _instance._uiInputs;
    
    public static void UpdateHousing()
    {
        _instance._population.UpdateHousing();
    }

    public static void UpdateResources(int crystal, int wood, int gold, int stone)
    {
        _instance._resources.UpdateResources(crystal, wood, gold, stone);
    }
}
