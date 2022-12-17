using UnityEngine;
using System.Collections.Generic;

public class HUDBuildings : HUD
{
    [SerializeField]
    private List<BuildingData> _economyBuildings = new List<BuildingData>();

    [SerializeField]
    private List<BuildingData> _militaryBuildings = new List<BuildingData>();

    [SerializeField]
    private List<BuildingButton> _economyButtons = new List<BuildingButton>();

    [SerializeField]
    private List<BuildingButton> _militaryButtons = new List<BuildingButton>();

    private void Awake()
    {
        for (int i = 0; i < _economyButtons.Count; i++)
            if (_economyBuildings[i] != null)
                _economyButtons[i].SetupButton(_economyBuildings[i]);
            else
                Destroy(_economyButtons[i]);

        for (int i = 0; i < _militaryButtons.Count; i++)
            if (_militaryBuildings[i] != null)
                _militaryButtons[i].SetupButton(_militaryBuildings[i]);
            else
                Destroy(_militaryButtons[i]);
    }
}
