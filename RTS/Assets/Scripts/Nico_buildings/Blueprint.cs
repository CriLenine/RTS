using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Blueprint : MonoBehaviour
{
    private static PeonBuilds _buildType;

    internal static Blueprint InstantiateWorldPos(PeonBuilds buildType)
    {
        _buildType = buildType;
        return Instantiate(PrefabManager.GetBuildingData(buildType).BuildingBlueprint, Vector2.zero, Quaternion.identity);
    }

    private void Start()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if(Physics2D.OverlapPoint(worldPos))
        {
            transform.position = worldPos;
        }

    }

    private void Update()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if (Physics2D.OverlapPoint(worldPos))
        {
            transform.position = worldPos;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            NetworkManager.Input(TickInput.Build((int)_buildType, transform.position));
            Destroy(gameObject);
        }
    }
}
