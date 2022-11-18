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

        /// <returns>The parent of the deleted node, which is <see langword="null"/> if the node was not found</returns>
        public Node<T> DeleteDescendant(T value)
        {
            if (_value.Equals(value))
            {
                foreach (Node<T> child in _children)
                {
                    child.Parent = Parent;
                    Parent._children.Add(child);
                }
                Parent.RemoveChild(this);
                return Parent;
            }
            else
            {
                Node<T> deletedNodeParent = null;
                foreach (Node<T> child in _children)
                {
                    deletedNodeParent ??= child.DeleteDescendant(value);
                    if (deletedNodeParent != null)
                        return deletedNodeParent;
                }
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

        _holyNode?.Children.Clear();
    }

    #region Baking

    [ContextMenu("Bake")]
    public void Bake(Vector2Int holyTree)
    {
        _holyNode = new Node<Vector2Int>(holyTree);

        _trees.Add(holyTree);

        RBuildTree(_holyNode, ^1, ySorted: _trees);
        //RPrintNode(_holyNode);
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

    private void RPrintNode(Node<Vector2Int> node, string offset = "")
    {
        Debug.Log($"{offset}{node.Value}");
        if (node.Children.Count == 0)
            return;
        offset += "       ";
        foreach (Node<Vector2Int> child in node.Children)
            RPrintNode(child, offset);
    }

    /// <summary>
    /// Removes the node <paramref name="lastTree"/> from the data tree
    /// </summary>
    /// <returns>The position of the next tree to cut, or <paramref name="lastTree"/> if forest is empty</returns>
    public Vector2Int GetNextTree(Vector2Int lastTree)
    {
        /*
         * Remarque : Si jamais l'arbre est mal construit, le nextTree pourrait ne pas �tre atteignable par le p�on
         * De plus, � tester sur war2combat : si jamais chemin bloqu� par ennemi ? par alli� ?
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

    public override void Tick()
    {
        throw new NotImplementedException();
    }

    public override Hash128 GetHash128()
    {
        throw new NotImplementedException();
    }
}