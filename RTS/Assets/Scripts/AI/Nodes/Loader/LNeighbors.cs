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

        HashSet<int> neighbourIds = new HashSet<int>();

        List<int> allies = new List<int>();
        List<int> enemmies = new List<int>();

        foreach (Character character in context.Characters)
            neighbourIds.AddRange(QuadTreeNode.GetNeighbours(character.ID, character.Position));

        foreach (int id in neighbourIds)
        {
            Character neighbor = GameManager.Characters[id];

            if (neighbor.Performer != context.Performer)
            {
                enemmies.Add(id);

                context.Enemies.Add(neighbor);
            }
            else
                allies.Add(id);
        }

        context.AllyIds = allies.ToArray();
        context.EnemyIds = enemmies.ToArray();

        return child.Update();
    }
}
