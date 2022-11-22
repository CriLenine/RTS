using System;
using System.Collections.Generic;
using UnityEngine;

public class Node<T>
{
    protected readonly T _value;

    protected readonly List<Node<T>> _children;

    public Node<T> Parent { get; protected set; }

    public T Value => _value;

    public Node<T> this[Index i] => _children[i];

    public bool IsLeave => _children.Count == 0;

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

        _children = children ?? new List<Node<T>>();
    }

    public Node<T> AddChild(T value)
    {
        Node<T> newNode = new Node<T>(value) { Parent = this };
        _children.Add(newNode);
        return newNode;
    }

    public void Clear()
    {
        _children.Clear();
    }

    /// <summary>Delete the node holding <paramref name="value"/> and replace it in the tree by his children.
    /// The targeted node must be the current node or in one of its branch.</summary>
    /// <returns>The parent of the deleted node, or <see langword="null"/> if the node was not found.</returns>
    public Node<T> DeleteDescendant(T value)
    {
        if (Equals(_value, value))
        {
            //Replace this node by its children
            foreach (Node<T> child in _children)
            {
                child.Parent = Parent;
                Parent._children.Add(child);
            }
            Parent._children.Remove(this);
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

    protected virtual bool Equals(T value1, T value2)
    {
        return value1.Equals(value2);
    }

    public void RPrintNode(string offset = "")
    {
        Debug.Log($"{offset}{_value}");
        if (_children.Count == 0)
            return;
        offset += "       ";
        foreach (Node<T> child in _children)
            child.RPrintNode(offset);
    }
}
