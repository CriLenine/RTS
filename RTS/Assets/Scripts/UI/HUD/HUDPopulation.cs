using UnityEngine;
using TMPro;

public class HUDPopulation : HUD
{
    [SerializeField]
    private TextMeshProUGUI _supplyBlockIndicatorUI;
    private int _maxHousing;

    [SerializeField]
    private TextMeshProUGUI _idleUnitsUI;
    private int _idleUnits = 0;

    public void UpdateHousing()
    {
        _supplyBlockIndicatorUI.text = $"{GameManager.MyCharacters.Count}/{GameManager.Housing}";
    }
}
