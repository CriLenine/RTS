using UnityEngine;
using MyBox;

public abstract class ButtonData : ScriptableObject
{
    [Separator("Button Binding")]
    public UnityEngine.UI.Button.ButtonClickedEvent OnClick;
}