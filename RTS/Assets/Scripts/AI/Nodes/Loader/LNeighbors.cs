using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;
using Unity.VisualScripting;

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
        context.Enemies.Clear();

        context.PeonCount = 0;
        context.SoldierCount = 0;

        HashSet<int> neighbourIds = new HashSet<int>();

        List<int> allies = new List<int>();
        List<int> enemmies = new List<int>();

        foreach (Character character in context.Characters)
        {
            neighbourIds.AddRange(QuadTreeNode.GetNeighbours(character.ID, character.Position));

            allies.Add(character.ID);

            if (character.Data.Type == Character.Type.Peon)
                ++context.PeonCount;
            else
                ++context.SoldierCount;
        }

        foreach (int id in neighbourIds)
        {
            Character neighbor = GameManager.Characters[id];

            if (neighbor.Performer != context.Performer)
            {
                enemmies.Add(id);

                context.Enemies.Add(neighbor);
            }
        }

        context.AllyIds = allies.ToArray();
        context.EnemyIds = enemmies.ToArray();

        return child.Update();
    }
}