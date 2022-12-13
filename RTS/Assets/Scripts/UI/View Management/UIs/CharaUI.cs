using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaUI : ViewManager
{
    private Character _character;
    public Character Character => _character;
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
        _character = uiOwner as Character;

        _title.text = _character.Data.name.ToString();
        //UtilViews = _character.Data.Views;
        base.ShowUI(uiOwner);
    }
}
