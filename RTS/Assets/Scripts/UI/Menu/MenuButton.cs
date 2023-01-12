using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private TextMeshProUGUI _text;

    [SerializeField]
    private Color _defaultTextColor, _hoveredTextColor;

    private void OnEnable()
    {
        _text.color = _defaultTextColor;
    }

    public void UpdateDefaultColor(Color color)
    {
        _defaultTextColor = color;
        _text.color = _defaultTextColor;
    }

    public void OnPointerEnter(PointerEventData _)
    {
        _text.DOColor(_hoveredTextColor, .2f);
        GameEventsManager.PlayEvent("Hover");
    }

    public void OnPointerExit(PointerEventData _)
    {
        _text.DOColor(_defaultTextColor, .2f);
    }
}
