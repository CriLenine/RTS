using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaUI : ViewManager
{
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
        Character chara = uiOwner as Character;

        _title.text = chara.Data.name.ToString();
        //UtilViews = chara.Data.Views;
        base.ShowUI(uiOwner);
    }
}
