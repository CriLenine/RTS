using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _pressAnyKey;

    [SerializeField]
    private CanvasGroup _strip;

    private void Update()
    {
        if(Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
        {
            _strip.gameObject.SetActive(true);
            _strip.DOFade(1f, 1f);
            _pressAnyKey.DOFade(0f, 1f).OnComplete(() => _pressAnyKey.gameObject.SetActive(false));
        }
    }
}
