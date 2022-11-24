using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawningView : View
{
    [SerializeField] private ItemUI _itemUI;
    [SerializeField] private Transform _itemsParent;
    public override void Initialize(ViewManager parentManager)
    {
        foreach (var chara in PrefabManager.DataCharacters)
        {
            var bUI = Instantiate(_itemUI);
            bUI.transform.SetParent(_itemsParent);
            bUI.InitUI(onClick, chara.CharaUiIcon, chara.name);
        }
    }

    private void onClick()
    {
        throw new NotImplementedException("bah alors");
    }
}
