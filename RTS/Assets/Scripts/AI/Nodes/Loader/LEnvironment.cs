using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;
using System;

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
        if (context.StartPosition is null)
        {
            context.StartPosition = context.Buildings.At(0).Position;
            context.StartCoords = context.Buildings.At(0).Coords;

            context.TargetMovePosition = context.StartPosition.Value;
        }

        if (context.Characters.Count == 0)
            return State.Failure;

        context.BalancePosition = (context.StartPosition.Value + context.Characters.At(0).Position) / 2f;

        Vector2Int coordOffset = Vector2Int.zero;

        foreach (Character character in context.Characters)
        {
            for (coordOffset.x = -character.ViewRadius; coordOffset.x <= character.ViewRadius; ++coordOffset.x)
            {
                for (coordOffset.y = -character.ViewRadius; coordOffset.y <= character.ViewRadius; ++coordOffset.y)
                {
                    if (coordOffset.x * coordOffset.x + coordOffset.y * coordOffset.y <= character.ViewRadius * character.ViewRadius)
                    {
                        Vector2Int viewCoords = character.Coords + coordOffset;

                        float sqrCoordsDistance = Vector2.SqrMagnitude(viewCoords - context.StartCoords);

                        if (ResourcesManager.HasTree(viewCoords))
                            context.KnownResources[ResourceType.Wood].Add(viewCoords, sqrCoordsDistance);
                        else if (ResourcesManager.HasCrystal(viewCoords))
                            context.KnownResources[ResourceType.Crystal].Add(viewCoords, sqrCoordsDistance);
                        else if (ResourcesManager.HasGold(viewCoords))
                            context.KnownResources[ResourceType.Gold].Add(viewCoords, sqrCoordsDistance);
                        else if (ResourcesManager.HasRock(viewCoords))
                            context.KnownResources[ResourceType.Stone].Add(viewCoords, sqrCoordsDistance);
                        else
                            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
                                context.KnownResources[type].Remove(viewCoords, sqrCoordsDistance);
                    }
                }
            }
        }

        return child.Update();
    }
}
