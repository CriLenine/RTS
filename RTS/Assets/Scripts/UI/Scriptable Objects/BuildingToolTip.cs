using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingToolTip", menuName = "HUD/BuildingToolTip", order = 2)]

public class BuildingToolTip : ToolTip
{
    public List<Resource.Amount> Cost;
    public int TimeCost;
    public string Description;
}
