using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class QuadTreeManager : MonoBehaviour
{
    public int nCharThresh = 3;
    public QuadTreeNode holyNode;
    public int x0;
    public int y0;
    public int j;
    public float width;
    public float height;

    public List<Transform> transformList;

    [ContextMenu("CreateQuadTree")]
    public void CreateQuadTree()
    {
        Stopwatch sw = Stopwatch.StartNew();
        holyNode = QuadTreeNode.Init(nCharThresh, x0, y0);
        sw.Stop();
        Debug.Log($"QuadTree created in {sw.Elapsed.TotalMilliseconds} ms, contains {holyNode.d_NodesCount} nodes, and a depth of {holyNode.d_Depth}.");
    }

    [ContextMenu("Update with j")]
    public void UpdateTreeJ()
    {
        string s = "";
        Stopwatch sw = Stopwatch.StartNew();
        HashSet<int> neighbors = holyNode.GetNeighbours(j, width, height, transformList[j].position, nCharThresh);
        sw.Stop();
        foreach (int neighbor in neighbors)
        {
            s += neighbor + ", ";
        }
        s += " ; leaves : ";
        foreach (var item in QuadTreeNode.d_Leaves[j])
        {
            s += item.d_name + ", ";
        }
        Debug.Log($"Neighbors of {j} : {s}");
        Debug.Log($"QuadTree updated in {sw.Elapsed.TotalMilliseconds} ms, contains {holyNode.d_NodesCount} nodes, and has a depth of {holyNode.d_Depth}.");
    }

    [ContextMenu("Update with all")]
    public void UpdateTree()
    {
        for (j = 0; j < transformList.Count; ++j)
            UpdateTreeJ();
    }

    [ContextMenu("ClearGizmos")]
    public void ClearGizmos()
    {
        GizmosManager.Clear();
    }
}
