using System.Collections.Generic;
using UnityEngine;



public class ButtonCustomization : ScriptableObject
{
    public ActionType _actionType;

    public Sprite Sprite;
    [DrawIf("_actionType", ActionType.Toogle)]
    public Sprite ToogledSprite;

    public Color Color;
}
