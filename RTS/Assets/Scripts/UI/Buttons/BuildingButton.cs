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

    private void Awake()
    {
        _image.color = _custom.Color;
        _image.sprite = _custom.Sprite;
    }
}
