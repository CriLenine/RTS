using UnityEngine;
using UnityEngine.EventSystems;

public enum ToolTipType
{
    Default,
    Stat,
    ActionTogglable,
    Action,
    Building,
    SpawnResearch
}

public class ToolTipElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private ToolTip _toolTip;
    public ToolTip ToolTip => _toolTip;

    public void Init(ToolTip toolTip)
    {
        _toolTip = toolTip;
        toolTip.Init();
    }

    public void OnPointerEnter(PointerEventData _)
    {
        switch (_toolTip.Type) 
        {
            case ToolTipType.Default:
                ToolTipManager.DisplayDefaultToolTip(_toolTip);
                break;
            case ToolTipType.Stat:
                ToolTipManager.DisplayStatToolTip((StatToolTip)_toolTip);
                break;
            case ToolTipType.Action:
                ToolTipManager.DisplayActionToolTip((ActionToolTip)_toolTip, false);
                break;
            case ToolTipType.ActionTogglable:
                ToolTipManager.DisplayActionToolTip((ActionToolTip)_toolTip, true);
                break;
            case ToolTipType.Building:
                ToolTipManager.DisplayBuildingToolTip((BuildingToolTip)_toolTip);
                break;
            case ToolTipType.SpawnResearch:
                ToolTipManager.DisplaySpawnToolTip((SpawnResearchToolTip)_toolTip);
                break;
        }
    }

    public void OnPointerExit(PointerEventData _)
    {
        ToolTipManager.HideToolTip();
    }
}
