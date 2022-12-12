using System;
using System.Collections.Generic;
using UnityEngine;

public class Node<T>
{
    public static Node<T> RootNode;

    private readonly T _value;

    private readonly List<Node<T>> _children;

    private Node<T> _parent;

    public T Value => _value;

    public Node<T> this[Index i] => _children[i];

    public bool IsLeaf => _children.Count == 0;

    public bool IsAvailable { get; private set; }

    public Node(T value, List<Node<T>> children = null)
    {
        _value = value;

        _children = children ?? new List<Node<T>>();

        IsAvailable = true;
    }

    public static void Init(T rootValue)
    {
        RootNode = new Node<T>(rootValue);
        RootNode.IsAvailable = false;
    }

    public static Node<T> RDisable(T rootValue)
    {
        return RootNode.RDisableDescendant(rootValue);
    }

    public Node<T> RGetClosest()
    {
        if (IsAvailable)
            return this;
        if (this == RootNode)
        {
            Debug.Log("No node available left in the data tree");
            return null;
        }
        Node<T> validNode = null;
        foreach (Node<T> child in _parent._children)
        {
            if (child == this)
                continue;
            validNode ??= child.RGetClosestChild();
        }
        return validNode ?? _parent.RGetClosest();
    }

    private Node<T> RGetClosestChild()
    {
        if (IsAvailable)
            return this;
        if (IsLeaf)
            return null;
        Node<T> validNode = null;
        foreach(Node<T> child in _children)
            validNode ??= child.RGetClosestChild();
        return validNode;
    }

    public Node<T> AddChild(T value)
    {
        Node<T> newNode = new Node<T>(value) { _parent = this };
        _children.Add(newNode);
        return newNode;
    }

    public void Clear()
    {
        _children.Clear();
    }

    /// <summary>Mark the node holding <paramref name="value"/> as unavailable.
    /// The targeted node must be the current node or in one of its branch.</summary>
    /// <returns>The node, or <see langword="null"/> if the node was not found.</returns>
    private Node<T> RDisableDescendant(T value)
    {
        if (Equals(_value, value))
        {
            IsAvailable = false;
            return this;
        }
        else
        {
            Node<T> disabledNode = null;
            foreach (Node<T> child in _children)
                disabledNode ??= child.RDisableDescendant(value);
            return disabledNode;
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
