using UnityEngine;
using TMPro;

public class HUDPopulation : HUD
{
    [SerializeField]
    private TextMeshProUGUI _supplyBlockIndicatorUI;

    [SerializeField]
    private TextMeshProUGUI _idleUnitsUI;
    private int _idleUnits = 0;

    public void UpdateHousing()
    {
        _supplyBlockIndicatorUI.text = $"{GameManager.MyCharacters.Count}/{GameManager.Housing}";
    }
}
