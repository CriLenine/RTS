using UnityEngine;

[CreateAssetMenu(fileName = "SpawnResearchToolTip", menuName = "HUD/ToolTips/SpawnResearchToolTip", order = 5)]

public class SpawnResearchToolTip : StatToolTip
{
    public CharacterData CharacterData;

    public override void Init()
    {
        Type = ToolTipType.SpawnResearch;
    }
}
