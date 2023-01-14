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
        if (context.StartPosition is null)
        {
            context.StartPosition = context.Buildings.At(0).Position;
            context.StartCoords = context.Buildings.At(0).Coords;

            context.TargetMovePosition = context.StartPosition.Value;
        }

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
                            context.Trees.Add(viewCoords, sqrCoordsDistance);
                        else if (ResourcesManager.HasCrystal(viewCoords))
                            context.Crystals.Add(viewCoords, sqrCoordsDistance);
                        else
                        {
                            if (context.Trees.Remove(viewCoords, sqrCoordsDistance))
                                context.cuttedTree = null;

                            context.Crystals.Remove(viewCoords, sqrCoordsDistance);
                        }
                    }
                }
            }
        }

        return child.Update();
    }
}
