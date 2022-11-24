using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionView : View
{
    [SerializeField] private ItemUI _buildingUI;
    [SerializeField] private Transform _buildingsParent;
    public override void Initialize(ViewManager parentManager) 
    {
        foreach (var building in PrefabManager.DataBuildings)
        {
            var bUI = Instantiate(_buildingUI);
            bUI.transform.SetParent(_buildingsParent);
            bUI.InitUI(BuildingBlueprintsManager.SpawnBlueprint, building.BuildingUiIcon, building.name, building.Type);
        }
    }
}
