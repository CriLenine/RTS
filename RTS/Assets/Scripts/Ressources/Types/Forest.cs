using System.Collections.Generic;
using UnityEngine;
using System;

public class Forest : Resource
{
    public override void OnHarvestedTile(Vector2Int coords)
    {
        ResourcesManager.RemoveTree(coords);
        LogicalTile harvestedTile = TileMapManager.GetLogicalTile(coords);
        harvestedTile.State = TileState.Free;
        harvestedTile.Tag = TileTag.None;
        CurrentAmount = CurrentAmount.RemoveQuantity(1);
    }
}
