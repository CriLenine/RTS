using UnityEngine;

[CreateAssetMenu(fileName = "ActionToolTip", menuName = "HUD/ToolTips/Building", order = 4)]

public class BuildingToolTip : StatToolTip
{
    public BuildingData BuildingData;

    public override void Init()
    {
        Type = ToolTipType.Building;
    }
}
