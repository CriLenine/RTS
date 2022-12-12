using System.Collections.Generic;
using UnityEngine;
using System;

public class Forest : Resource
{
    public override void Bake()
    {
        
    }

    public override void OnHarvestedTile(Vector2Int coords)
    {
        GameManager.ResourcesManager.RemoveTree(coords);
        TileMapManager.GetLogicalTile(coords).State = TileState.Free;
        CurrentAmount = CurrentAmount.RemoveQuantity(1);
    }
}
