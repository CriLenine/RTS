using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawningView : View
{
    [SerializeField] 
    private ItemUI _itemUI;
    [SerializeField] 
    private Transform _itemsParent;

    [SerializeField]
    private Image _slider;

    private BuildingUI _manager;
    private ISpawner _spawner = null;

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
    private void Update()
    {
        _slider.fillAmount = _spawner is null ? 0 : _spawner.SpawningTime;
    }
    private void OnClick(Character.Type type)
    {
        if(_manager.Building.TryGetComponent(out ISpawner spawner))
        {
            _spawner = spawner;
            spawner.EnqueueSpawningCharas(type);
        }
    }

}
