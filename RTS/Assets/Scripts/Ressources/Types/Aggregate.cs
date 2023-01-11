using System.Collections.Generic;
using UnityEngine;

public class Aggregate : Resource
{
    public override void OnHarvestedTile(Vector2Int coords, bool tileDepleted)
    {
        CurrentAmount = CurrentAmount.RemoveQuantity(Data.AmountPerHarvest);

        ResourcesManager.UpdateTile(coords, Data.Type, Data.TileAspects, false, (float)_itemsNHarvested[coords] / Data.NMaxHarvestPerTile);

        if (tileDepleted)
        {
            TileMapManager.GetLogicalTile(coords).Tag = TileTag.None;

            _itemsNHarvested.Remove(coords);
        }
    }
}
