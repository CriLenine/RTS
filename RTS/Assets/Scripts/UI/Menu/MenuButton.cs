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

    public void OnPointerEnter(PointerEventData _)
    {
        _text.DOColor(_hoveredTextColor, .2f);
    }

    public void OnPointerExit(PointerEventData _)
    {
        _text.DOColor(_defaultTextColor, .2f);
    }
}
