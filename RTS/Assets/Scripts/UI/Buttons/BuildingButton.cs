using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    [SerializeField]
    private ButtonCustomization _custom;

    [SerializeField]
    private Image _image;

    [SerializeField]
    private Button _button;

    [SerializeField]
    private ToolTip _toolTip;
    public ToolTip ButtonToolTip => _toolTip;

    private void Awake()
    {
        _image.color = _custom.Color;
        _image.sprite = _custom.Sprite;

        ToolTipElement toolTipElement = null;

        if (_toolTip != null)
        {
            toolTipElement = gameObject.AddComponent<ToolTipElement>();
            toolTipElement.Init(_toolTip);
        }

        if (_button != null && (toolTipElement == null || toolTipElement.ToolTip.Type != ToolTipType.Building))
            _button.interactable = false;
    }
    public void SetButtonInteractability(bool value)
    {
        _button.interactable = value;
    }

}
