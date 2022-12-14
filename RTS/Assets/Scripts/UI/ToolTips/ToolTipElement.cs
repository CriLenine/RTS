using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    enum ToolTipType
    {
        Default,
        Stat,
        ActionTogglable,
        Action,
        Building,
        SpawnResearch
    }

    [SerializeField]
    private ToolTipType _toolTipType;

    [SerializeField]
    private ToolTip _tooltip;

    public void OnPointerEnter(PointerEventData _)
    {
        switch (_toolTipType) 
        {
            case ToolTipType.Default:
                ToolTipManager.DisplayDefaultToolTip(_tooltip);
                break;
            case ToolTipType.Stat:
                ToolTipManager.DisplayStatToolTip((StatToolTip)_tooltip);
                break;
            case ToolTipType.Action:
                ToolTipManager.DisplayActionToolTip((ActionToolTip)_tooltip, false);
                break;
            case ToolTipType.ActionTogglable:
                ToolTipManager.DisplayActionToolTip((ActionToolTip)_tooltip, true);
                break;
            case ToolTipType.Building:
                ToolTipManager.DisplayBuildingToolTip((BuildingToolTip)_tooltip);
                break;
            case ToolTipType.SpawnResearch:
                ToolTipManager.DisplaySpawnToolTip((SpawnResearchToolTip)_tooltip);
                break;
        }
    }

    public void OnPointerExit(PointerEventData _)
    {
        ToolTipManager.HideToolTip();
    }
}
