using UnityEngine;
using System.Collections.Generic;

public class HUDBuildings : HUD
{
    [SerializeField]
    private List<BuildingButton> _dynamicButtons = new List<BuildingButton>();

    private void Update()
    {
        if (HUDSection.activeInHierarchy)
            foreach (BuildingButton button in _dynamicButtons)
                button.SetButtonInteractability(GetInteractabilityValue(button));
    }

    private bool GetInteractabilityValue(BuildingButton button)
    {
        BuildingToolTip toolTip = button.ButtonToolTip as BuildingToolTip;
        BuildingData data = toolTip.BuildingData;

        foreach (Resource.Amount cost in data.Cost)
            if (GameManager.MyResources[cost.Type] < cost.Value)
                return false;

        return true;
    }
}
