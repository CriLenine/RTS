using UnityEngine;
using TMPro;

public class HUDResources : HUD
{
    [SerializeField]
    private TextMeshProUGUI[] _resourcesCountsUI, _assignedPeonsUI;

    public void UpdateResources(int crystal, int wood, int gold, int stone)
    {
        _resourcesCountsUI[0].text = crystal.ToString();
        _resourcesCountsUI[1].text = wood.ToString();
        _resourcesCountsUI[2].text = gold.ToString();
        _resourcesCountsUI[3].text = stone.ToString();
    }
}
