using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PeonUtils
{
    Builds,
    Die
}

public class PeonView : View
{
    [SerializeField] private GameObject _depth0;
    [SerializeField] private GameObject _buildings;

    [SerializeField] private Transform _buildinglayoutParent;
    [SerializeField] private BuildingUI _buildingUI;

    public override void Initialize()
    {
        foreach (var building in PrefabManager.GetBuildingDatas)
        {
            var bUI = Instantiate(_buildingUI);
            bUI.transform.SetParent(_buildinglayoutParent);
            bUI.InitUI(building.BuildingUiIcon, building.name,building.Type);
        }
    }

    public void OnDepth0Click(int index)
    {
        switch ((PeonUtils)index)
        {
            case PeonUtils.Builds:
                _depth0.SetActive(false);
                _buildings.SetActive(true);
                break;
            case PeonUtils.Die:
                break;
        }
    }


}
