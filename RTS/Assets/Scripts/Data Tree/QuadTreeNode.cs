using System;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEditor;
using UnityEngine;

public class QuadTreeNode
{
    public struct CharacterInfo
    {
        public int ID;
        public float xMin;
        public float xMax;
        public float yMax;
        public float yMin;
    }

    private static int _nCharactersThreshold;
    private static Dictionary<int, HashSet<QuadTreeNode>> _leaves;
    private static float x0, y0;

    private readonly HashSet<CharacterInfo> _characters;

    private QuadTreeNode _leftChild;
    private QuadTreeNode _rightChild;

    public HashSet<CharacterInfo> Characters => _characters;

    public QuadTreeNode LeftChild => _leftChild;
    public QuadTreeNode RightChild => _rightChild;

    public bool IsLeave => _leftChild == null;//Cannot have only 1 child so no need to check rightChild

    public int NodesCount => 1 + (IsLeave ? 0 : _leftChild.NodesCount + _rightChild.NodesCount);

    public QuadTreeNode(HashSet<CharacterInfo> value, QuadTreeNode leftChild = null, QuadTreeNode rightChild = null)
    {
        _characters = value;

        _leftChild = leftChild;
        _rightChild = rightChild;
    }

    public void Clear()
    {
        _leftChild = null;
        _rightChild = null;
    }

    public void RPrintNode(string offset = "")
    {
        Debug.Log($"{offset}{_characters}");
        if (!IsLeave)
            offset += "       ";
        _leftChild.RPrintNode(offset);
        _rightChild.RPrintNode(offset);
    }

    /// <summary>
    /// Called when moving a character.
    /// <para>Updates the tree with the character's new position.</para>
    /// </summary>
    /// <param name="ID">The ID of the moved character.</param>
    /// <returns>All the IDs of characters in same leaves than <paramref name="character"/></returns>
    public HashSet<int> GetNeighbours(int ID, float xMin, float xMax, float yMin, float yMax)
    {
        return GetNeighbours(new CharacterInfo { ID=ID, xMin=xMin, xMax=xMax, yMin=yMin, yMax=yMax });
    }
    
    private HashSet<int> GetNeighbours(CharacterInfo character)
    {
        UpdateTree(character);
        HashSet<int> neighbours = new HashSet<int>();
        foreach (QuadTreeNode leave in _leaves[character.ID])
            foreach (CharacterInfo characterInfo in leave.Characters)
                if (characterInfo.ID != character.ID)
                    neighbours.Add(characterInfo.ID);
        return neighbours;
    }
    /// <summary>
    /// Called in <see cref="GetNeighbours(CharacterInfo)"/>.
    /// <para>Places the character in its new correct leaves, then updates <see cref="_leaves"/>.</para>
    /// </summary>
    private void UpdateTree(CharacterInfo character)
    {
        HashSet<QuadTreeNode> newLeaves = RPlace(character, x0, y0);
        HashSet<QuadTreeNode> currentLeaves = _leaves[character.ID];
        foreach (QuadTreeNode node in currentLeaves)
        {
            if (!newLeaves.Contains(node))
                _leaves[character.ID].Remove(node);
        }
    }

    /// <summary>
    /// Recursive.
    /// <para>Called on each tree update.</para>
    /// Runs through the tree to place the <paramref name="character"/> into the correct leaves, creating ones if necessary.
    /// </summary>
    /// <returns>A <see cref="HashSet{QuadTreeNode}"/> containing the leaves where the <paramref name="character"/> is stocked.</returns>
    private HashSet<QuadTreeNode> RPlace(CharacterInfo character, float x, float y, int depth = 0)
    {
        HashSet<QuadTreeNode> nodes = new HashSet<QuadTreeNode>();
        if (IsLeave)
        {
            if (_characters.Count + 1 <= _nCharactersThreshold)
            {
                _characters.Add(character);
                _leaves[character.ID].Add(this);
                nodes.Add(this);
            }
            else
            {
                HashSet<CharacterInfo> leftCharacters = new HashSet<CharacterInfo>();
                HashSet<CharacterInfo> rightCharacters = new HashSet<CharacterInfo>();

                if (depth % 2 == 0)//Discriminate via x
                {
                    foreach (CharacterInfo characterToSort in _characters)
                    {
                        if (characterToSort.xMin < x)
                            leftCharacters.Add(characterToSort);
                        if (characterToSort.xMax > x)
                            rightCharacters.Add(characterToSort);
                    }
                }
                else //Discriminate via y
                {
                    foreach (CharacterInfo characterToSort in _characters)
                    {
                        if (characterToSort.yMin < y)
                            leftCharacters.Add(characterToSort);
                        if (characterToSort.yMax > y)
                            rightCharacters.Add(characterToSort);
                    }
                }
                _leftChild = new QuadTreeNode(leftCharacters);
                _rightChild = new QuadTreeNode(rightCharacters);
            }
        }
        if (!IsLeave)
        {
            float depthMultiplicator = 1 / Mathf.Pow(2, depth / 2);
            if (depth % 2 == 0)//Discriminate via x
            {
                if (character.xMin < x)
                    nodes.IntersectWith(_leftChild.RPlace(character, x - x0 * depthMultiplicator, y, depth + 1));
                if (character.xMax > x)
                    nodes.IntersectWith(_rightChild.RPlace(character, x + x0 * depthMultiplicator, y, depth + 1));
            }
            else//Discriminate via y
            {
                if (character.yMin < y)
                    nodes.IntersectWith(_leftChild.RPlace(character, x, y - y0 * depthMultiplicator, depth + 1));
                if (character.yMax > y)
                    nodes.IntersectWith(_rightChild.RPlace(character, x, y + y0 * depthMultiplicator, depth + 1));
            }
        }
        return nodes;
    }
}
