using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ToolTip", menuName = "HUD/ToolTips/Default", order = 1)]
public class ToolTip : ScriptableObject
{
    public string Name;

    [HideInInspector]
    public ToolTipType Type;

    public virtual void Init()
    {
        Type = ToolTipType.Default;
    }
}
