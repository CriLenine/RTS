using System.Collections.Generic;
using System.Collections;
using System;

public class ResourceQueue<T> : IEnumerable<T>
{
    private class Node : IComparable<Node>
    {
        public T Item { get; private set; }
        public float Priority { get; private set; }

        public Node(T item, float priority)
        {
            Item = item;
            Priority = priority;
        }

        public int CompareTo(Node other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }

    private List<Node> _list = new List<Node>();
    private HashSet<T> _guard = new HashSet<T>();

    public int Count => _list.Count;

    public bool Add(T item, float priority)
    {
        bool alreadyExists = !_guard.Add(item);

        if (alreadyExists)
            return false;

        Node node = new Node(item, priority);

        int index = _list.BinarySearch(node);

        if (index >= 0)
        {
            _list.Insert(index + 1, node);
        }
        else
        {
            _list.Insert(~index, node);
        }

        return true;
    }

    public bool Remove(T item, float priority)
    {
        bool doesntExist = !_guard.Remove(item);

        if (doesntExist)
            return false;

        Node node = new Node(item, priority);

        int index = _list.BinarySearch(node);

        if (_list[index].Item.Equals(item))
            _list.RemoveAt(index);

        int i = index;

        while (i > 0 && _list[i].Priority == priority)
        {
            if (_list[i].Item.Equals(item))
            {
                _list.RemoveAt(i);
                
                return true;
            }

            --i;
        }

        i = index + 1;

        while (i < _list.Count && _list[i].Priority == priority)
        {
            if (_list[i].Item.Equals(item))
            {
                _list.RemoveAt(i);

                return true;
            }

            ++i;
        }

        return false;
    }

    public T Dequeue()
    {
        if (Count > 0)
        {
            T item = _list[0].Item;

            _list.RemoveAt(0);

            _guard.Remove(item);

            return item;
        }

        return default(T);
    }

    public T Peek()
    {
        return Count >= 0 ? _list[0].Item : default(T);
    }

    public T this[int i] => _list[i].Item;

    public IEnumerator<T> GetEnumerator()
    {
        foreach (Node node in _list)
            yield return node.Item;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}