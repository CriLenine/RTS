using UnityEngine;

public class Forest : Resource
{
    public override void OnHarvestedTile(Vector2Int coords, bool tileDepleted)
    {
        CurrentAmount = CurrentAmount.RemoveQuantity(Data.AmountPerHarvest);

        ResourcesManager.UpdateTile(coords, Data.Type, Data.TileAspects, tileDepleted, (float)_itemsNHarvested[coords] / Data.NMaxHarvestPerTile);

        if (tileDepleted)
        {
            LogicalTile harvestedTile = TileMapManager.GetLogicalTile(coords);
            harvestedTile.Tag = TileTag.None;
            harvestedTile.State = TileState.Free;

            _itemsNHarvested.Remove(coords);
        }
    }
}
