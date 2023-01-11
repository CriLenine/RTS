using System;
using UnityEngine;
using System.Collections.Generic;
using MyBox;

public class HUDManager : MonoBehaviour
{
    private static HUDManager _instance;

    #region Variables

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
    private HUDSelection _selection;

    [SerializeField]
    private HUDStats _stats;

    [SerializeField]
    private HUDActions _actions;

    [SerializeField]
    private HUDSpawnPreview _spawnPreview;

    [SerializeField]
    private HUDBuildings _buildings;

    [SerializeField]
    private HUDTime _time;

    [SerializeField]
    private HUDMinimap _minimap;

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

    #endregion

    protected void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this);

        ShowDefaultHUD();

        foreach (ResourceSpecs spec in _resourcesSpecs)
        {
            _resourceSprites.Add(spec.Type, spec.Sprite);
            _resourceColors.Add(spec.Type, spec.Color);
        }
    }

    public static void DisplayStats(Character character)
    {
        _instance._stats.DisplayStats(character);
    }

    public static void DisplayStats(Building building)
    {
        _instance._stats.DisplayStats(building);
    }

    public static void HideAll()
    {
        _instance._time.Hide();
        _instance._stats.Hide();
        _instance._actions.Hide();
        _instance._minimap.Hide();
        _instance._buildings.Hide();
        _instance._selection.Hide();
        _instance._resources.Hide();
        _instance._population.Hide();
        _instance._spawnPreview.Hide();
    }

    public static void ShowDefaultHUD()
    {
        _instance._time.Show();
        _instance._minimap.Show();
        _instance._resources.Show();
        _instance._population.Show();
    }

    public static void UpdateHUD()
    {
        ToolTipManager.HideToolTip();

        List<Character> characters = SelectionManager.SelectedCharacters;
        Building building = SelectionManager.SelectedBuilding;

        _instance._stats.Hide();
        _instance._actions.Hide();
        _instance._buildings.Hide();
        _instance._selection.Hide();
        _instance._spawnPreview.Hide();

        int charactersCount = characters.Count;

        if (charactersCount == 0 && building == null)
            return;

        _instance._actions.Show();

        if (building != null)
        {
            _instance._stats.DisplayStats(building);
            _instance._spawnPreview.Show();
            _instance._spawnPreview.UpdateSpawnPreview();
            _instance._actions.ShowBuildingActions(building);
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
        _instance._selection.Show();

        _instance._selection.SetupSelection();

        for (int i = 0; i < charactersCount; ++i)
            if (!characters[i].Data.CanBuild)
            {
                _instance._buildings.Hide();
                return;
            }
    }

    public static void UpdateHousing()
    {
        _instance._population.UpdateHousing();
    }

    public static void UpdateResources(int crystal, int wood, int gold, int stone)
    {
        _instance._resources.UpdateResources(crystal, wood, gold, stone);
    }

    public static void UpdateSpawnPreview()
    {
        _instance._spawnPreview.UpdateSpawnPreview();
    }

    public static void StartTimer()
    {
        _instance._time.StartTimer();
        _instance._time.Show();
    }

    public static void StopTimer()
    {
        _instance._time.StopTimer();
    }

    public static string GetTimer()
    {
        return _instance._time.GetTimer();
    }
}
