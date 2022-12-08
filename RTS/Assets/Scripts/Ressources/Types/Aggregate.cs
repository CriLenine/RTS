using System.Collections.Generic;
using UnityEngine;

public class Aggregate : Resource
{
    public override void HarvestedTile(Vector2Int coords)
    {
        CurrentAmount = CurrentAmount.RemoveQuantity(Data.AmountPerHarvest);
    }
}
