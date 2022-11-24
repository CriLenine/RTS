using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingUI : ViewManager
{
    public override void ShowUI<T>(T uiOwner)
    {
        Building chara = uiOwner as Building;

        _title.text = chara.Data.name.ToString();
        UtilViews = chara.Data.Views;
        base.ShowUI(uiOwner);
    }
}
