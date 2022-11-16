using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ForestRessource : Ressource
{
    private class Node<T>
    {
        private readonly T _value;

        private List<Node<T>> _children;

        public Node<T> Parent { get; private set; }

        public T Value => _value;

        public Node<T> this[Index i] => _children[i];

        public List<Node<T>> Children => _children;

        public int NodesCount 
        { 
            get 
            {
                int sum = 0;
                foreach (Node<T> child in _children)
                    sum += child.NodesCount;
                return 1 + sum; 
            } 
        }

        public Node(T value, List<Node<T>> children = null)
        {
            _value = value;

            _children = children ?? new();
        }

        public bool RemoveChild(Node<T> node)
        {
            return _children.Remove(node);
        }

        public Node<T> AddChild(T value)
        {
            Node<T> newNode = new Node<T>(value) { Parent = this };
            _children.Add(newNode);
            return newNode;
        }

        /// <returns>The parent of the deleted node</returns>
        public Node<T> DeleteDescendant(T value)
        {
            if (_value.Equals(value))
            {
                foreach (Node<T> child in _children)
                {
                    child.Parent = Parent;
                    Parent._children.Add(child);
                }
                return Parent;
            }
            else
            {
                Node<T> deletedNodeParent = null;
                foreach (Node<T> child in _children)
                    deletedNodeParent ??= child.DeleteDescendant(value);
                return deletedNodeParent;                
            }
        }
    }

    [SerializeField]
    private List<Vector2Int> _trees;

    private Node<Vector2Int> _holyNode;

    public void AddTree(Vector2Int newTree) => _trees.Add(newTree);

    public void Clear()
    {
        _trees.Clear();

        // TODO : Finir
    }

    #region Baking

    [ContextMenu("Bake")]
    public void Bake()
    {
        Vector2Int holyTree = Vector2Int.FloorToInt(transform.position);

        _holyNode = new Node<Vector2Int>(holyTree);

        _trees.Add(holyTree);

        RBuildTree(_holyNode, ^1, ySorted: _trees);
        //RPrintNode(_holyNode);
    }

    private void RBuildTree(Node<Vector2Int> parent, Index index, List<Vector2Int> xSorted = null, List<Vector2Int> ySorted = null)
    {
        if (xSorted == null)
        {
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
            List<Vector2Int> xSortedRight = ySorted.GetRange(newIndex + 1, ySorted.Count - newIndex);
            List<Vector2Int> xSortedLeft = ySorted.GetRange(0, newIndex);
            RBuildTree(nextNode, 0, xSorted: xSortedRight);
            RBuildTree(nextNode, ^1, xSorted: xSortedLeft);
        }
        else
        {
            if (ySorted.Count == 1)
            {
                parent.AddChild(ySorted[0]);
                return;
            }

            Vector2Int nextValue = xSorted[index];
            Node<Vector2Int> nextNode = parent.AddChild(nextValue);

            //xSorted becomes y-sorted
            xSorted.Sort((Vector2Int a, Vector2Int b) => a.y < b.y ? -1 : a.y > b.y ? 1 : 0);

            int newIndex = xSorted.FindIndex(x => x == nextValue);
            List<Vector2Int> ySortedRight = xSorted.GetRange(newIndex, xSorted.Count - newIndex + 1);
            List<Vector2Int> ySortedLeft = xSorted.GetRange(0, newIndex + 1);
            RBuildTree(nextNode, 0, ySorted: ySortedRight);
            RBuildTree(nextNode, ^1, ySorted: ySortedLeft);
        }
    }
    #endregion

    private void RPrintNode(Node<Vector2Int> node)
    {
        if (node.Children.Count == 0)
            return;
        foreach (Node<Vector2Int> child in node.Children)
        {
            Debug.Log($"Node : {node.Value} contains {child.Value} which holds : ");
            RPrintNode(child);
        }
    }
    
    /// <returns>The destination of the peon to cut the tree at <paramref name="treePosition"/>.</returns>
    public override Vector2Int GetHarvestingPosition(Vector2Int treePosition)
    {
        Vector2Int currentPos = treePosition;
        List<Vector2Int> availableTiles = new();
        //Check all the outlines around the tree
        for (int outline = 1; outline <= /*TileMapManager.MaxValue*/ 5; ++outline)
        {
            //Run through each tile of the current outline
            for (int i = -outline; i <= outline; ++i)
            {
                for (int j = -outline; j <= outline; ++j)
                {
                    if (i == 0 && j == 0)
                        continue;
                    Vector2Int tilePosition = currentPos + new Vector2Int(i, j);
                    if (TileMapManager.GetTile(tilePosition).State == TileState.Free)
                        availableTiles.Add(tilePosition);
                }
            }
            //If we found at least one candidate
            if (availableTiles.Count > 0)
            {
                //Find the nearest candidate to the tree in magnitude
                (int minMagnitude, int index) = ((availableTiles[0] - treePosition).sqrMagnitude, 0);
                for (int i = 1; i < availableTiles.Count; i++)
                {
                    int currentMagnitude = (availableTiles[i] - treePosition).sqrMagnitude;
                    if (currentMagnitude < minMagnitude)
                        (minMagnitude, index) = (currentMagnitude, i);
                    /*
                     * TODO : À magnitudes égales, choisir le plus proche du joueur via pathfinding (optionnel)   
                     */
                }
                return availableTiles[index];
            }
        }
        Debug.LogError("No free tile found !");
        return treePosition;
    }

    /// <summary>
    /// Removes the node <paramref name="lastTree"/> from the data tree
    /// </summary>
    /// <returns>The position of the next tree to cut, or <paramref name="lastTree"/> if forest is empty</returns>
    public Vector2Int GetNextTree(Vector2Int lastTree)
    {
        /*
         * Remarque : Si jamais l'arbre est mal construit, le nextTree pourrait ne pas être atteignable par le péon
         * De plus, à tester sur war2combat : si jamais chemin bloqué par ennemi ? par allié ?
         */
        /* 
         * TODO : update la tilemap
         */
        Node<Vector2Int> substituteParent = _holyNode.DeleteDescendant(lastTree);
        if (_holyNode.NodesCount == 1)
            return lastTree;
        if (substituteParent.Children.Count > 0)
            return substituteParent[0].Value;
        return substituteParent.Value;
    }

    protected override void Tick()
    {
        throw new System.NotImplementedException();
    }

    protected override Hash128 GetHash128()
    {
        throw new System.NotImplementedException();
    }
}
