using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class QuadTreeDebugger : MonoBehaviour
{
    public int j;
    public string node;

    [ContextMenu("GetNeighbours j")]
    public void UpdateTreeJ()
    {
        string s = "";
        Stopwatch sw = Stopwatch.StartNew();
        HashSet<int> neighbors = QuadTreeNode.GetNeighbours(j, GameManager.Entities[j].transform.position);
        sw.Stop();
        foreach (int neighbor in neighbors)
        {
            s += neighbor + ", ";
        }
        s += " ; leaves : ";
        foreach (var item in QuadTreeNode.d_Leaves[j])
        {
            s += item.ID + ", ";
        }
        Debug.Log($"Neighbors of {j} : {s}");
        Debug.Log($"got in {sw.Elapsed.TotalMilliseconds}");
    }

    [ContextMenu("Get characters node")]
    public void GetCharacters()
    {
        string s = "";
        Stopwatch sw = Stopwatch.StartNew();
        HashSet<int> neighbors = QuadTreeNode.QuadTreeRoot.RGetCharacters(node);
        sw.Stop();
        foreach (int neighbor in neighbors)
        {
            s += neighbor + ", ";
        }

        Debug.Log($"Characters of {node} : {s}");
        Debug.Log($"got in {sw.Elapsed.TotalMilliseconds}");
    }
}
