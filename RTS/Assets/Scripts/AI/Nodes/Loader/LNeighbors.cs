using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;
using Unity.VisualScripting;
using System;

[System.Serializable]
public class LNeighbors : DecoratorNode
{
    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (context.Characters.Count == 0)
            return State.Failure;

        foreach (Building building in context.Buildings)
            if (!building.BuildComplete)
                return State.Success;

        foreach (Character.Type type in Enum.GetValues(typeof(Character.Type)))
            context.CharacterCount[type] = 0;

        context.Leader = context.Characters.At(0);

        context.Enemies.Clear();

        HashSet<int> neighbourIds = new HashSet<int>();

        List<int> allies = new List<int>();
        List<int> enemmies = new List<int>();

        foreach (Character character in context.Characters)
        {
            neighbourIds.AddRange(QuadTreeNode.GetNeighbours(character.ID, character.Position));

            allies.Add(character.ID);

            ++context.CharacterCount[character.Data.Type];
        }

        foreach (int id in neighbourIds)
        {
            Character neighbor = GameManager.Characters[id];

            if (neighbor.Performer == context.Performer)
                continue;

            if (Vector2.SqrMagnitude(neighbor.Position - context.Leader.Position) > 16f)
                continue;

            enemmies.Add(id);

            context.Enemies.Add(neighbor);
        }

        context.AllyIds = allies.ToArray();
        context.EnemyIds = enemmies.ToArray();

        return child.Update();
    }
}
