using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawningView : View
{
    [SerializeField] private ItemUI _itemUI;
    [SerializeField] private Transform _itemsParent;
    private BuildingUI _manager;
    public override void Initialize<T>(T parentManager)
    {
        _manager = parentManager as BuildingUI;

        foreach (var chara in PrefabManager.DataCharacters)
        {
            var bUI = Instantiate(_itemUI);
            bUI.transform.SetParent(_itemsParent);
            bUI.InitUI(OnClick, chara.CharaUiIcon, chara.name,chara.Type);
        }
    }

    private void OnClick(Character.Type type)
    {
        NetworkManager.Input(TickInput.Spawn((int)type, (Vector2)_manager.Building.transform.position+new Vector2(0.5f,0.5f)));
    }
}
