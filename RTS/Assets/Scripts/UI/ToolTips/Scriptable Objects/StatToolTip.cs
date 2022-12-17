using UnityEngine;

[CreateAssetMenu(fileName = "StatToolTip", menuName = "HUD/ToolTips/Stat", order = 2)]

public class StatToolTip : ToolTip
{
    [TextArea(3, 10)]
    public string Description;

    public override void Init()
    {
        Type = ToolTipType.Stat;
    }
}
