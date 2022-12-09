using UnityEngine;
using TMPro;

public class HUDPopulation : HUD
{
    private static HUDPopulation _instance;

    [SerializeField]
    private TextMeshProUGUI _supplyBlockIndicatorUI;
    private int _unitsCount = 0;
    private int _maxHousing = 0;

    [SerializeField]
    private TextMeshProUGUI _idleUnitsUI;
    private int _idleUnits = 0;

    protected void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this);
    }

    public static void UpdateSupplyBlockIndicator(int unitsDeltaValue, int MaxHousingDeltaValue)
    {
        _instance._unitsCount += unitsDeltaValue;
        _instance._maxHousing += MaxHousingDeltaValue;
        _instance._supplyBlockIndicatorUI.text = $"{_instance._unitsCount}/{_instance._maxHousing}";
    }

    public static void UpdateIdleUnits(int deltaValue)
    {
        _instance._idleUnits += deltaValue;
        _instance._idleUnitsUI.text = _instance._idleUnits.ToString();
    }
}
