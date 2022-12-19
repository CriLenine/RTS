using UnityEngine;
using System.Collections.Generic;

public class HUDBuildings : HUD
{
    [SerializeField]
    private List<BuildingButton> _economyButtons, _militaryButtons;

    [SerializeField]
    private List<ButtonDataHUDParameters> _economyBuildings, _militaryBuildings;

    private void Awake()
    {
        foreach(ButtonDataHUDParameters parameters in _economyBuildings)
            _economyButtons[parameters.ButtonPosition.x + parameters.ButtonPosition.y * 4].SetupButton(parameters.ButtonData as BuildingData);

        foreach(ButtonDataHUDParameters parameters in _militaryBuildings)
            _militaryButtons[parameters.ButtonPosition.x + parameters.ButtonPosition.y * 4].SetupButton(parameters.ButtonData as BuildingData);
    }
}
