using System.Collections.Generic;
using TheKiwiCoder;
using Unity.VisualScripting;

[System.Serializable]
public class AScanNearEnvironement : ActionNode
{
    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        HashSet<int> neighbourIds = new HashSet<int>();

        foreach (Character character in blackboard.Characters)
            neighbourIds.AddRange(QuadTreeNode.GetNeighbours(character.ID, character.Position));

        foreach (int id in neighbourIds)
        {

        }

        return State.Success;
    }
}
