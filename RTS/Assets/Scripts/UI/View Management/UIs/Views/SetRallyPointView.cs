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

    private bool _started = false;
    private LineRenderer _pathRenderer;
    public override void Initialize<T>(T parentManager)
    {
        _manager = parentManager as BuildingUI;

        _lastPos = Vector2.zero;
        _blockClick = true;
    }

    private void Update()
    {
        if(!_started)
        {
            _started = true;
            _pathRenderer = _manager.Building.PathRenderer;
        }

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 newMousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        _pathRenderer.SetPosition(0, _manager.Building.transform.position);
        _pathRenderer.SetPosition(1, newMousePos);

        _pathRenderer.transform.position = newMousePos;

        _pathRenderer.startColor = Color.cyan;
        _pathRenderer.endColor = Color.cyan;

        _pathRenderer.gameObject.SetActive(true);


        if (newMousePos != _lastPos)
        {
            _lastPos = newMousePos;
            text.text = "New rally point is : " + TileMapManager.WorldToTilemapCoords(newMousePos);
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2Int tile = TileMapManager.WorldToTilemapCoords(_lastPos);

            if (!(TileMapManager.GetLogicalTile(tile)?.IsFree(NetworkManager.Me) == true))
                return;

            if (_manager.Building.TryGetComponent(out ISpawner spawner))
            {
                spawner.SetRallyPoint(_lastPos);
                _started = false;
                UIManager.CurrentManager.ShowLast();
            }
            else throw new System.Exception("building is not spawner");
        }
    }
}
