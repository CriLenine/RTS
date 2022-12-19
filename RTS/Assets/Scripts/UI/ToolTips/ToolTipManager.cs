using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ToolTipManager : MonoBehaviour
{
    private Mouse _mouse;

    private static ToolTipManager _instance;

    [SerializeField]
    private List<ToolTipVisualBind> toolTipVisualBinds;

    private Dictionary<ToolTipType, ToolTipVisual> toolTipVisuals = new Dictionary<ToolTipType, ToolTipVisual>();

    private List<ToolTipType> _visualTypes = new List<ToolTipType>();

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

        foreach (ToolTipVisualBind bind in toolTipVisualBinds)
        {
            toolTipVisuals.Add(bind.Type, bind.Visual);
            _visualTypes.Add(bind.Type);
        }
    }

    public static void DisplayToolTip(ToolTip toolTip)
    {
        _instance._currentToolTipVisual = _instance.toolTipVisuals[toolTip.Type];

        _instance.ShowToolTip(toolTip.Type);

        _instance._currentToolTipVisual.Name.text = toolTip.Name;

        if (toolTip.Type == ToolTipType.Default)
            return;

        _instance._currentToolTipVisual.Description.text = toolTip.Description;

        if (toolTip.Type == ToolTipType.Stat || toolTip.Type == ToolTipType.Action)
            return;

        TextMeshProUGUI[] costTexts = _instance._currentToolTipVisual.ResourceCostTexts.Value;
        Image[] costIcons = _instance._currentToolTipVisual.ResourceCostIcons.Value;

        Resource.Amount[] costs;

        if (toolTip.Type == ToolTipType.Building)
        {
            _instance._currentToolTipVisual.Icon.sprite = toolTip.BuildingData.HUDIcon;
            _instance._currentToolTipVisual.Icon.color = toolTip.BuildingData.SubType == SubType.Economy ? HUDManager.EconomyTypeColor : HUDManager.MilitaryTypeColor;

            costs = toolTip.BuildingData.Cost;
        }
        else
        {
            _instance._currentToolTipVisual.Icon.sprite = toolTip.Data.Icon;
            _instance._currentToolTipVisual.Icon.color = toolTip.Data.SubType == SubType.Economy ? HUDManager.EconomyTypeColor : HUDManager.MilitaryTypeColor;

            costs = toolTip.Data.Cost;
        }

        for (int i = 0; i < _instance._currentToolTipVisual.ResourceCostTexts.Value.Length; ++i)
            if (i < costs.Length)
            {
                costTexts[i].text = $"{costs[i].Value}";
                costTexts[i].color = new Color(1, 1, 1, 1);
                costIcons[i].sprite = GameManager.ResourcesSprites[(int)costs[i].Type];
            }
            else
            {
                costTexts[i].text = string.Empty;
                costIcons[i].color = new Color(0, 0, 0, 0);
            }

        if (toolTip.Type == ToolTipType.Building)
            return;

        int seconds = Mathf.CeilToInt(toolTip.Data.SpawnTicks * NetworkManager.TickPeriod);
        _instance._currentToolTipVisual.TimeCost.text = $"{seconds / 60}m{seconds % 60}s";
    }

    [Serializable]
    struct ToolTipVisualBind
    {
        public ToolTipType Type;
        public ToolTipVisual Visual;
    }

    private void ShowToolTip(ToolTipType wantedType)
    {
        toolTipVisuals[wantedType].gameObject.SetActive(true);
        _instance._displayed = true;
    }

    public static void HideToolTip()
    {
        if (_instance._currentToolTipVisual != null)
            _instance._currentToolTipVisual.gameObject.SetActive(false);

        _instance._displayed = false;
    }

    private void Update()
    {
        if (_instance._displayed)
            _instance._currentToolTipVisual.gameObject.transform.position = _instance._offset + _mouse.position.ReadValue();
    }
}
