using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionView : View
{
    [SerializeField] private ItemUI _buildingUI;
    [SerializeField] private Transform _buildingsParent;

    private Dictionary<BuildingData, ItemUI> _buildingsUi;
    private CharaUI _manager;
    public override void Initialize<T>(T parentManager) 
    {
        _manager = parentManager as CharaUI;

        _buildingsUi = new();

        foreach (var building in PrefabManager.DataBuildings)
        {
            var bUI = Instantiate(_buildingUI);
            bUI.transform.SetParent(_buildingsParent);
            bUI.InitUI(BuildingBlueprintsManager.SpawnBlueprint, building.BuildingUiIcon, building.name, building.Type);

            bUI.gameObject.SetActive(false);

            _buildingsUi.Add(building,bUI);
        }
    }

    public override void Show()
    {
        var data = _manager.Character.Data as PeonData; //AIE AIE AIE

        foreach (var buildingUi in _buildingsUi)
        {
            if (data.Buildable.Contains(buildingUi.Key.Type))
                buildingUi.Value.gameObject.SetActive(true);
        }

        base.Show();
    }
}
