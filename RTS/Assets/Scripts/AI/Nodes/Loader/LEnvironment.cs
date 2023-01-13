using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

[System.Serializable]
public class LEnvironment : DecoratorNode
{
    protected override void OnStart()
    {

    }

    protected override void OnStop()
    {

    }

    protected override State OnUpdate()
    {
        Vector2Int? targetCoords = TileMapManager.GetNearestFogCoords(context.Performer, context.Characters[0].Position);

        Debug.Log(targetCoords);

        if (targetCoords == null)
            return State.Failure;

        context.Inputs.Add(TickInput.Move(context.AllyIds, TileMapManager.TilemapCoordsToWorld(targetCoords.Value), context.Performer));

        return State.Success;
    }
}
