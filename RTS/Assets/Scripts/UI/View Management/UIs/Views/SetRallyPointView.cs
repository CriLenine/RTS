using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class SetRallyPointView : View
{
    [SerializeField]
    private TextMeshProUGUI text;
    private BuildingUI _manager;
    private Vector2 _lastPos;
    public override void Initialize<T>(T parentManager)
    {
        _manager = parentManager as BuildingUI;

        _lastPos = Vector2.zero;
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 newMousePos = Mouse.current.position.ReadValue();
        if (newMousePos != _lastPos)
        {
            _lastPos = newMousePos;
            text.text = "New rally point is : " + TileMapManager.WorldToTilemapCoords(Camera.main.ScreenToWorldPoint(newMousePos));
        }

        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (_manager.Building.TryGetComponent(out ISpawner spawner))
                spawner.SetRallyPoint(Camera.main.ScreenToWorldPoint(_lastPos));
        }
    }
}
