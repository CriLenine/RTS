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

    private ToolTipElement toolTipElement = null;

    private CharacterData _data;

    private GameObject _buttonFill;

    private bool _isToggle = false;

    public void SetupButton(ButtonData data)
    {
        if (data is ConstantButtonData currentActionData)
        {
            _image.color = currentActionData.Color;
            _image.sprite = currentActionData.Icon;

            if (currentActionData.ButtonType == ButtonType.Regular)
            {
                if (_button == null)
                    _button = gameObject.AddComponent<Button>();

                _button.onClick = data.OnClick;

                TryDestroyLongClickButton();
            }
            else
            {
                if (_buttonFill == null)
                    _buttonFill = Instantiate(HUDManager.ButtonFill);

                if (_longClickButton == null)
                    _longClickButton = gameObject.AddComponent<LongClickButton>();

                _buttonFill.transform.SetParent(gameObject.transform, false);

                _longClickButton.FillImage = _buttonFill.GetComponent<Image>();

                _longClickButton.RequiredHoldTime = currentActionData.HoldTime;

                _longClickButton.OnLongClick = data.OnClick;
            }

            SetupToolTip(currentActionData.ToolTip);
        }
        else if(data is CharacterData characterData)
        {
            _image.color = characterData.Color;
            _image.sprite = characterData.Icon;

            if (_button == null)
                _button = gameObject.AddComponent<Button>();

            _button.onClick = data.OnClick;

            TryDestroyLongClickButton();

            SetupToolTip(characterData.ToolTip);
        }
    }

    public void ResetButton()
    {
        _image.sprite = null;
        _image.color = HUDManager.DefaultButtonColor;

        if(_button != null)
            Destroy(_button);

        TryDestroyLongClickButton();
    }

    private void TryDestroyLongClickButton()
    {
        if (_longClickButton != null)
            Destroy(_longClickButton);

        if(_buttonFill != null)
            Destroy(_buttonFill);
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
