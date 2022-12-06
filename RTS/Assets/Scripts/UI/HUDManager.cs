using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public enum Resource
    {
        cristal,
        wood,
        gold,
        stone
    }

    private static HUDManager _instance;

    [SerializeField] 
    private TextMeshProUGUI[] _resourcesCountsUI;
    private int[] _resourcesCount;

    [SerializeField]
    private TextMeshProUGUI[] _assignedPeonsUI;
    private int[] _assignedPeons;

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

        _resourcesCount = new int[_instance._resourcesCountsUI.Length];
        _assignedPeons = new int[_instance._assignedPeonsUI.Length];
    }

    public static void UpdateResource(Resource resource, int deltaValue)
    {
        _instance._resourcesCount[(int)resource] += deltaValue;
        _instance._resourcesCountsUI[(int)resource].text = _instance._resourcesCount[(int)resource].ToString();
    }

    public static void UpdateAssignment(Resource resource, int deltaValue)
    {
        _instance._assignedPeons[(int)resource] += deltaValue;
        _instance._assignedPeonsUI[(int)resource].text = _instance._assignedPeons[(int)resource].ToString();
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
