using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingUI : ViewManager
{
    private Building _building;
    public Building Building => _building;

    public override void Initialize()
    {
        for (int i = 0; i < _views.Length; i++)
        {
            _views[i].Initialize(this);
            _views[i].Hide();
        }
    }

    public override void ShowUI<T>(T uiOwner)
    {
        Building building = uiOwner as Building;

        _title.text = building.Data.name.ToString();
        _building = building;
        base.ShowUI(uiOwner);
    }
}
