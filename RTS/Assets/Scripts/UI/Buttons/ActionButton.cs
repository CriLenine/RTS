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
    private ButtonCustomization _custom;

    [SerializeField]
    private Image _image;

    [SerializeField]
    private Button _button;
    public Button Button => _button;

    private bool _isToogle = false;
    private void Awake()
    {
        _image.color = _custom.Color;
        _image.sprite = _custom.Sprite;
    }

    public bool ToogleButton()
    {
        if(_isToogle)
        {
            _isToogle = false;
            _image.sprite = _custom.Sprite;
        }
        else
        {
            _isToogle = true;
            _image.sprite = _custom.ToogledSprite;
        }

        return _isToogle;
    }

}
