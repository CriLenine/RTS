using TheKiwiCoder;
using UnityEngine;

[System.Serializable]
public class AExplore : ActionNode
{
    protected override void OnStart()
    {

    }

    protected override void OnStop()
    {

    }

    protected override State OnUpdate()
    {
        if (GameManager.PlayerResources[ResourceType.Wood][context.Performer] < 1025 && context.Trees.Count > 0)
        {
            if (context.Characters.At(0).CurrentAction is Harvest)
                return State.Success;

            Vector2Int nearestTree = context.Trees.Peek();

            GameManager.dCoords = nearestTree;

            Debug.Log($"Look for trees {nearestTree.x} {nearestTree.y}");

            context.Inputs.Add(TickInput.Harvest(nearestTree, context.AllyIds, context.Performer));

            context.cuttedTree = nearestTree;

            return State.Success;
        }

        Vector2Int? fogCoords = TileMapManager.GetNearestFogCoords(context.Performer, context.BalancePosition);

        Debug.Log("Look for fog");

        if (fogCoords is null)
            return State.Failure;

        context.TargetMovePosition = Vector2.Lerp(context.TargetMovePosition, TileMapManager.TilemapCoordsToWorld(fogCoords.Value), 0.1f);

        context.Inputs.Add(TickInput.Move(context.AllyIds, context.TargetMovePosition, context.Performer));

        return State.Success;
    }
}
