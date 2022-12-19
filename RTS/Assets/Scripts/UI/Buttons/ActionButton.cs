using UnityEngine;
using UnityEngine.UI;

public enum ActionType
{
    Trigger,
    Toogle
}

public class ActionButton : MonoBehaviour
{
    [SerializeField]
    private Image _image;

    private Button _button;

    private LongClickButton _longClickButton;

    private GameObject _buttonFill;

    private ToolTipElement toolTipElement = null;

    private CharacterData _data;

    private bool _isToggle = false;

    public void SetupButton(ButtonData data)
    {
        if (data is ConstantButtonData constantButtonData)
        {
            _image.color = constantButtonData.Color;
            _image.sprite = constantButtonData.Icon;

            if (constantButtonData.ButtonType == ButtonType.Regular)
            {
                if(_button == null)
                    _button = gameObject.AddComponent<Button>();

                if (data.OnClick != null)
                    _button.onClick = data.OnClick;

                if(_longClickButton != null)
                    DestroyLongClickButton();
            }
            else
            {
                _buttonFill = Instantiate(HUDManager.ButtonFill);
                _buttonFill.transform.SetParent(transform, false);

                if(_longClickButton == null)
                    _longClickButton = gameObject.AddComponent<LongClickButton>();

                _longClickButton.FillImage = _buttonFill.GetComponent<Image>();

                _longClickButton.RequiredHoldTime = constantButtonData.HoldTime;
                _longClickButton.OnLongClick = data.OnClick;
            }

            SetupToolTip(constantButtonData.ToolTip);
        }
        else if(data is CharacterData characterData)
        {
            if (_button == null)
                _button = gameObject.AddComponent<Button>();

            _image.color = characterData.Color;
            _image.sprite = characterData.Icon;

            if(data.OnClick != null)
                _button.onClick = data.OnClick;

            DestroyLongClickButton();

            SetupToolTip(characterData.ToolTip);
        }
    }

    public void ResetButton()
    {
        _image.sprite = null;
        _image.color = HUDManager.DefaultButtonColor;

        if(_button != null)
            Destroy(_button);

        DestroyLongClickButton();

        if(toolTipElement != null)
            Destroy(toolTipElement);
    }

    private void DestroyLongClickButton()
    {
        if (_longClickButton != null)
        {
            Destroy(_longClickButton);
            Destroy(_buttonFill);
        }
    }

    private void SetupToolTip(ToolTip toolTip)
    {
        if (toolTip != null)
        {
            if (toolTipElement == null)
                toolTipElement = gameObject.AddComponent<ToolTipElement>();

            toolTipElement.Init(toolTip);
        }
        else if (toolTipElement != null)
            Destroy(toolTipElement);

    }

    private void Update()
    {
        if (_data != null)
            _button.interactable = TestInteractability();
    }

    private bool TestInteractability()
    {
        foreach (Resource.Amount cost in _data.Cost)
            if (GameManager.MyResources[cost.Type] < cost.Value)
                return false;
        return true;
    }

    public bool ToogleButton()
    {
        //if (_isToogle)
        //{
        //    _isToogle = false;
        //    _image.sprite = _custom.Sprite;
        //}
        //else
        //{
        //    _isToogle = true;
        //    _image.sprite = _custom.ToogledSprite;
        //}

        //return _isToogle;

        return _isToggle;
    }

}
