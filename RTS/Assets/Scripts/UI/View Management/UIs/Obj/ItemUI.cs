using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    [SerializeField] private Image _image;

    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Button _button;
    public void InitUI<T>(System.Action<T> onClick , Sprite sprite, string name, T parameter)
    {
        _image.sprite = sprite;
        _text.text = name;
        gameObject.transform.localScale = Vector3.one;
        _button.onClick.AddListener(() => onClick(parameter));
    }
    public void InitUI(System.Action onClick, Sprite sprite, string name)
    {
        _image.sprite = sprite;
        _text.text = name;
        gameObject.transform.localScale = Vector3.one;
        _button.onClick.AddListener(() => onClick());
    }
}
