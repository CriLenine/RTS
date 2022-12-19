using UnityEngine;
using UnityEngine.EventSystems;

public enum ToolTipType
{
    Default,
    Stat,
    Action,
    Building,
    SpawnResearch
}

public class ToolTipElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private ToolTip _toolTip;

    public void Init(ToolTip toolTip)
    {
        _toolTip = toolTip;
    }

    public void OnPointerEnter(PointerEventData _)
    {
        ToolTipManager.DisplayToolTip(_toolTip);
    }

    public void OnPointerExit(PointerEventData _)
    {
        ToolTipManager.HideToolTip();
    }
}
