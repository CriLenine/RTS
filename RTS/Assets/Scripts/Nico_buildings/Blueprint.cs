using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class Blueprint : MonoBehaviour
{
    private static Building.Type _buildType;
    private SpriteRenderer _blueprintRenderer;

    [SerializeField] 
    private Color _availableColor;
    [SerializeField]
    private Color _notAvailableColor;


    internal static Blueprint InstantiateWorldPos(Building.Type buildType)
    {
        SpawnableDataBuilding building = PrefabManager.GetBuildingData(buildType);

        _buildType = buildType;
        

        return Instantiate(building.BuildingBlueprint, Vector2.zero, Quaternion.identity);
    }

    private void Start()
    {
        _blueprintRenderer = GetComponent<SpriteRenderer>();

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if(Physics2D.OverlapPoint(worldPos))
        {
            transform.position = worldPos;
        }
    }

    private void Update()
    {
        /*int oC = 0;
        for (float i = 0.5f * TileMapManager._tileSize; i < _blueprintRenderer.bounds.extents.x; i+= TileMapManager._tileSize)
        {
            oC++;
        }*/

        int outlineCount = Mathf.FloorToInt(_blueprintRenderer.bounds.extents.x / TileMapManager._tileSize);

        (Vector3 position, bool available) = TileMapManager.TilesAvailableForBuild(outlineCount);

        if (Physics2D.OverlapPoint(position))
        {
            transform.position = position;
        }

        _blueprintRenderer.color = available ? _availableColor : _notAvailableColor;


        if (Mouse.current.leftButton.wasPressedThisFrame && available)
        {
            int[] selected = CharacterSelectionManager.GetSelectedIds();

            NetworkManager.Input(TickInput.Build(selected, (int)_buildType, transform.position));

            Destroy(gameObject);
        }
    }
}
