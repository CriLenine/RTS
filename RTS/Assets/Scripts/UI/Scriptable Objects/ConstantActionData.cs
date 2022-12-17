using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ButtonType
{
    Regular,
    LongClick
}

[CreateAssetMenu(fileName = "ConstantActionData", menuName = "HUD/ConstantActionData", order = 1)]

public class ConstantActionData : ActionData
{
    public ButtonType ButtonType;
    public Sprite Icon;
    public Color Color;
    public ToolTip ToolTip;
    public float HoldTime;
}
