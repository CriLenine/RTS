using System.Collections.Generic;
using UnityEngine;
using System;

public class Forest : Resource
{
    #region Baking

    public override void Bake()
    {
        Vector2Int coords = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        Node<Vector2Int>.Init(coords);

        _items.Add(coords, 0);

        RBuildTree(Node<Vector2Int>.RootNode, ^1, ySorted: new List<Vector2Int>(_items.Keys));
        //RPrintNode(_holyNode);
    }

    public override Vector2Int? GetNext(Vector2Int lastHarvested, Vector2Int attractionPoint, int performer)
    {
        Node<Vector2Int> lastNode = Node<Vector2Int>.RDisable(lastHarvested);
        return lastNode.RGetClosest().Value;
    }

    private void RBuildTree(Node<Vector2Int> parent, Index index, List<Vector2Int> xSorted = null, List<Vector2Int> ySorted = null)
    {
        if (xSorted == null)
        {
            if (ySorted.Count == 0)
                return;
            if (ySorted.Count == 1)
            {
                parent.AddChild(ySorted[0]);
                return;
            }

            Vector2Int nextValue = ySorted[index];
            Node<Vector2Int> nextNode = parent.AddChild(nextValue);

            //ySorted becomes x-sorted
            ySorted.Sort((Vector2Int a, Vector2Int b) => a.x < b.x ? -1 : a.x > b.x ? 1 : 0);

            int newIndex = ySorted.FindIndex(x => x == nextValue);
            List<Vector2Int> xSortedRight = ySorted.GetRange(newIndex + 1, ySorted.Count - newIndex - 1);
            List<Vector2Int> xSortedLeft = ySorted.GetRange(0, newIndex);
            RBuildTree(nextNode, 0, xSorted: xSortedRight);
            RBuildTree(nextNode, ^1, xSorted: xSortedLeft);
        }
        else
        {
            if (xSorted.Count == 0)
                return;
            if (xSorted.Count == 1)
            {
                parent.AddChild(xSorted[0]);
                return;
            }

            Vector2Int nextValue = xSorted[index];
            Node<Vector2Int> nextNode = parent.AddChild(nextValue);

            //xSorted becomes y-sorted
            xSorted.Sort((Vector2Int a, Vector2Int b) => a.y < b.y ? -1 : a.y > b.y ? 1 : 0);

            int newIndex = xSorted.FindIndex(x => x == nextValue);
            List<Vector2Int> ySortedRight = xSorted.GetRange(newIndex + 1, xSorted.Count - newIndex - 1);
            List<Vector2Int> ySortedLeft = xSorted.GetRange(0, newIndex);
            RBuildTree(nextNode, 0, ySorted: ySortedRight);
            RBuildTree(nextNode, ^1, ySorted: ySortedLeft);
        }
    }
    #endregion

    public override void OnHarvestedTile(Vector2Int coords)
    {
        GameManager.ResourcesManager.RemoveTree(coords);
        TileMapManager.GetLogicalTile(coords).State = TileState.Free;
        CurrentAmount = CurrentAmount.RemoveQuantity(1);
    }
}
