using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReturnUI : MonoBehaviour
{
    [SerializeField] private Button _button;

    public void Start()
    {
        _button.onClick.AddListener(() => UIManager.CurrentManager.ShowLast());
    }

}
