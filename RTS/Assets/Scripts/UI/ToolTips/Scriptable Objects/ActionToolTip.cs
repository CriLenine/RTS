using UnityEngine;

[CreateAssetMenu(fileName = "ActionToolTip", menuName = "HUD/ToolTips/Action", order = 3)]

public class ActionToolTip : StatToolTip
{
    public override void Init()
    {
        Type = ToolTipType.Action;
    }
}
