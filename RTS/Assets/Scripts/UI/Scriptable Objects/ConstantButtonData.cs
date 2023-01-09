using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public enum ButtonType
{
    Regular,
    LongClick
}

[CreateAssetMenu(fileName = "ConstantButtonData", menuName = "HUD/ConstantButtonData", order = 1)]

public class ConstantButtonData : ButtonData
{
    [Separator("Button Specs")]

    public ButtonType ButtonType;

    [ConditionalField(nameof(ButtonType), false, ButtonType.Regular)]
    public bool IsTogglable=false;

    [ConditionalField(nameof(ButtonType), false, ButtonType.LongClick)]
    public float HoldTime;

    [Space]

    [Separator("Art")]

    public Sprite Icon;
    public Color Color;

    [Space]

    [Separator("ToolTip")]

    public ToolTip ToolTip;
}
