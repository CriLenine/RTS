using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private ToolTip _tooltip;

    public void OnPointerEnter(PointerEventData _)
    {
        ToolTipManager.DisplayDefaultToolTip(_tooltip);
    }

    public void OnPointerExit(PointerEventData _)
    {
        ToolTipManager.HideToolTip();
    }
}
