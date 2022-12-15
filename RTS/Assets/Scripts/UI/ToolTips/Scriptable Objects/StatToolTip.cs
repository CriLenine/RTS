using UnityEngine;

[CreateAssetMenu(fileName = "StatToolTip", menuName = "HUD/ToolTips/Stat", order = 2)]

public class StatToolTip : ToolTip
{
    public string Description;

    public override void Init()
    {
        Type = ToolTipType.Stat;
    }
}
