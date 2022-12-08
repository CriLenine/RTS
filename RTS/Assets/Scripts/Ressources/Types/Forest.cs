using System.Collections.Generic;
using UnityEngine;
using System;

public class Forest : Resource
{
    public override void HarvestedTile(Vector2Int coords)
    {
        GameManager.ResourcesManager.RemoveTree(coords);
        CurrentAmount = CurrentAmount.RemoveQuantity(1);
    }
}
