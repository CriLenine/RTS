using System;
using UnityEngine;
using MyBox;


public enum SubType
{
    Economy,
    Military
}

[Serializable]
public struct ButtonDataHUDParameters
{
    public ButtonData ButtonData;
    public Vector2Int ButtonPosition;
}

public abstract class ButtonData : ScriptableObject
{
    [Separator("Button Binding")]
    public UnityEngine.UI.Button.ButtonClickedEvent OnClick;
}
