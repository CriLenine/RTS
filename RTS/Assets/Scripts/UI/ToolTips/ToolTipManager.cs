using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ToolTipManager : MonoBehaviour
{
    private Mouse _mouse;

    private static ToolTipManager _instance;

    [SerializeField]
    private ToolTipVisual _defaultToolTipVisual;
    [SerializeField]
    private StatToolTipVisual _statToolTipVisual;
    [SerializeField]
    private ActionToolTipVisual _actionToolTipVisual;
    [SerializeField]
    private BuildingToolTipVisual _buildingToolTipVisual;
    [SerializeField]
    private SpawnResearchToolTipVisual _spawnResearchToolTipVisual;

    private ToolTipVisual _currentToolTipVisual;

    private bool _displayed = false;

    private Vector2 _offset = new Vector2(.1f, .1f);

    protected void Awake()
    {
        _mouse = Mouse.current;

        if (_instance == null)
            _instance = this;
        else
            Destroy(this);
    }

    public static void DisplayDefaultToolTip(ToolTip toolTip)
    {
        _instance._displayed = true;
        _instance._defaultToolTipVisual.Visual.SetActive(true);
        _instance._defaultToolTipVisual.Name.text = toolTip.Name;

        _instance._currentToolTipVisual = _instance._defaultToolTipVisual;
    }

    public static void DisplayStatToolTip(StatToolTip toolTip)
    {
        _instance._displayed = true;
        _instance._statToolTipVisual.Visual.SetActive(true);
        _instance._statToolTipVisual.Name.text = toolTip.Name;
        _instance._statToolTipVisual.Description.text = toolTip.Description;

        _instance._currentToolTipVisual = _instance._statToolTipVisual;
    }

    public static void DisplayActionToolTip(ActionToolTip toolTip, bool togglable)
    {
        _instance._displayed = true;
        _instance._actionToolTipVisual.Visual.SetActive(true);
        _instance._actionToolTipVisual.Name.text = toolTip.Name;
        _instance._actionToolTipVisual.Description.text = toolTip.Description;
        //if (togglable)
        //    _instance._actionToolTipVisual.ToggledStatus.text = "Toggle status : " + (true ? "enabled" : "disabled");
        //else
        //    _instance._actionToolTipVisual.ToggledStatus.text = "";

        _instance._currentToolTipVisual = _instance._actionToolTipVisual;
    }

    public static void DisplayBuildingToolTip(BuildingToolTip toolTip)
    {
        BuildingData data = toolTip.BuildingData;

        _instance._displayed = true;
        _instance._buildingToolTipVisual.Visual.SetActive(true);
        _instance._buildingToolTipVisual.Icon.sprite = data.HUDIcon;
        _instance._buildingToolTipVisual.Name.text = toolTip.Name;
        _instance._buildingToolTipVisual.Description.text = toolTip.Description;

        for (int i = 0; i < _instance._buildingToolTipVisual.ResourceCostTexts.Length; ++i)
            if (i < data.Cost.Length)
            {
                _instance._buildingToolTipVisual.ResourceCostTexts[i].text = $"{data.Cost[i].Value}";
                _instance._buildingToolTipVisual.ResourceCostIcons[i].sprite = GameManager.ResourcesSprites[(int)data.Cost[i].Type];
                _instance._buildingToolTipVisual.ResourceCostIcons[i].color = new Color(1,1,1,1);
            }
            else
            {
                _instance._buildingToolTipVisual.ResourceCostTexts[i].text = string.Empty ;
                _instance._buildingToolTipVisual.ResourceCostIcons[i].color = new Color(0,0,0,0);
            }

        _instance._currentToolTipVisual = _instance._buildingToolTipVisual;
    }

    public static void DisplaySpawnToolTip(SpawnResearchToolTip toolTip)
    {
        CharacterData data = toolTip.CharacterData;

        _instance._displayed = true;
        _instance._spawnResearchToolTipVisual.Visual.SetActive(true);
        _instance._spawnResearchToolTipVisual.Icon.sprite = data.Icon;
        _instance._spawnResearchToolTipVisual.Name.text = toolTip.Name;
        _instance._spawnResearchToolTipVisual.Description.text = toolTip.Description;

        for (int i = 0; i < _instance._spawnResearchToolTipVisual.ResourceCostTexts.Length; ++i)
            if (i < data.Cost.Length)
            {
                _instance._spawnResearchToolTipVisual.ResourceCostTexts[i].text = $"{data.Cost[i].Value}";
                _instance._spawnResearchToolTipVisual.ResourceCostIcons[i].sprite = GameManager.ResourcesSprites[(int)data.Cost[i].Type];
                _instance._spawnResearchToolTipVisual.ResourceCostIcons[i].color = new Color(1, 1, 1, 1);
            }
            else
            {
                _instance._spawnResearchToolTipVisual.ResourceCostTexts[i].text = string.Empty;
                _instance._spawnResearchToolTipVisual.ResourceCostIcons[i].color = new Color(0, 0, 0, 0);
            }


        int seconds = Mathf.CeilToInt(data.SpawnTicks * NetworkManager.TickPeriod);

        _instance._spawnResearchToolTipVisual.TimeCost.text = $"{seconds / 60}m{seconds % 60}s";

        _instance._currentToolTipVisual = _instance._spawnResearchToolTipVisual;
    }

    public static void HideToolTip()
    {
        if(_instance._currentToolTipVisual != null)
            _instance._currentToolTipVisual.Visual.SetActive(false);
    }

    private void Update()
    {
        if (_instance._displayed)
            _instance._currentToolTipVisual.Visual.transform.position = _instance._offset + _mouse.position.ReadValue();
    }
}
