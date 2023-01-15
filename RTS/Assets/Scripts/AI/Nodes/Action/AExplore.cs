using TheKiwiCoder;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class AExplore : ActionNode
{
    protected override State OnUpdate()
    {
        Vector2Int? fogCoords = TileMapManager.GetNearestFogCoords(context.Performer, context.BalancePosition);

        if (fogCoords is null)
        {
            if (log)
                Debug.Log("No fog found");

            return State.Failure;
        }

        if (log)
            Debug.Log("Explore");

        context.TargetMovePosition = Vector2.Lerp(context.TargetMovePosition, TileMapManager.TilemapCoordsToWorld(fogCoords.Value), 0.1f);

        context.Inputs.Add(TickInput.Move(context.AllyIds, context.TargetMovePosition, context.Performer));

        return State.Success;
    }
}
