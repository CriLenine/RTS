using System.Collections.Generic;
using UnityEngine;

public class Aggregate : Resource
{
    public override void OnHarvestedTile(Vector2Int coords, int harvestedAmount, bool tileDepleted)
    {
        CurrentAmount = CurrentAmount.RemoveQuantity(harvestedAmount);

        if (tileDepleted)
        {
            TileMapManager.GetLogicalTile(coords).Tag = TileTag.None;
            _itemsNHarvested.Remove(coords);
        }
    }
}
