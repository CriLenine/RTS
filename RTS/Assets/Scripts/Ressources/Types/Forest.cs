using UnityEngine;

public class Forest : Resource
{
    public override void OnHarvestedTile(Vector2Int coords, int harvestedAmount, bool tileDepleted)
    {
        CurrentAmount = CurrentAmount.RemoveQuantity(harvestedAmount);
        if (tileDepleted)
        {
            LogicalTile harvestedTile = TileMapManager.GetLogicalTile(coords);
            harvestedTile.Tag = TileTag.None;
            harvestedTile.State = TileState.Free;
            ResourcesManager.RemoveTree(coords);
            _itemsNHarvested.Remove(coords);
        }
    }
}
