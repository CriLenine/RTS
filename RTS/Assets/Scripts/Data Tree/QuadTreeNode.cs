using System;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEditor;
using UnityEngine;

public class QuadTreeNode
{
    private static int _nCharactersThreshold;
    private static float x0, y0;
    private static Dictionary<int, HashSet<QuadTreeNode>> _leaves;

    private struct CharacterInfo
    {
        public readonly int ID;
        public readonly float xMin;
        public readonly float xMax;
        public readonly float yMax;
        public readonly float yMin;

        public CharacterInfo(int _ID, float _xMin, float _xMax, float _yMin, float _yMax)
        {
            ID = _ID;
            xMin = _xMin;
            xMax = _xMax;
            yMin = _yMin;
            yMax = _yMax;
        }
    }
    private readonly HashSet<CharacterInfo> _characters;

    private QuadTreeNode _leftChild;
    private QuadTreeNode _rightChild;

    public static Dictionary<int, HashSet<QuadTreeNode>> d_Leaves => _leaves;
    public readonly string d_name = "";

    private bool _IsLeave => _leftChild == null;//Cannot have only 1 child so no need to check rightChild

    #region Debug
    public int d_NodesCount => 1 + (_IsLeave ? 0 : _leftChild.d_NodesCount + _rightChild.d_NodesCount);
    public int d_Depth => _IsLeave ? 0 : 1 + Mathf.Max(_leftChild.d_Depth, _rightChild.d_Depth);
    #endregion

    private QuadTreeNode(string d_name, HashSet<CharacterInfo> value = null, QuadTreeNode leftChild = null, QuadTreeNode rightChild = null)
    {
        _characters = value ?? new HashSet<CharacterInfo>();
        _leftChild = leftChild;
        _rightChild = rightChild;
        this.d_name = d_name;
    }

    /// <summary>
    /// Creates a first node (the root) and initializes the essential variables.
    /// </summary>
    /// <param name="nCharactersThreshold">The maximum of characters we want in the same node</param>
    /// <param name="x0">The x corresponding to the middle of the area</param>
    /// <param name="y0">The y corresponding to the middle of the area</param>
    /// <returns>An empty <see cref="QuadTreeNode"/>.</returns>
    public static QuadTreeNode Init(int nCharactersThreshold, float x0, float y0)
    {
        _nCharactersThreshold = nCharactersThreshold;
        QuadTreeNode.x0 = x0;
        QuadTreeNode.y0 = y0;
        _leaves = new Dictionary<int, HashSet<QuadTreeNode>>();
        return new QuadTreeNode("r");
    }

    /// <summary>
    /// Called when moving a character.
    /// <para>Updates the tree with the character's new position.</para>
    /// </summary>
    /// <param name="ID">The ID of the moved character.</param>
    /// <returns>All the IDs of characters in same leaves than the selected character.</returns>
    public HashSet<int> GetNeighbours(int ID, float width, float height, Vector2 coords, int nCharactersThreshold = 0)
    {
        if (nCharactersThreshold != 0)
            _nCharactersThreshold = nCharactersThreshold;
        return GetNeighbours(new CharacterInfo(
                                        ID,
                                        coords.x + x0 - width / 2, 
                                        coords.x + x0 + width / 2, 
                                        coords.y + y0 - height / 2, 
                                        coords.y + y0 + height / 2));
    }
    
    private HashSet<int> GetNeighbours(CharacterInfo character)
    {
        UpdateTree(character);
        HashSet<int> neighbours = new HashSet<int>();
        foreach (QuadTreeNode leave in _leaves[character.ID])
            foreach (CharacterInfo characterInfo in leave._characters)
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
        HashSet<QuadTreeNode> currentLeaves = new HashSet<QuadTreeNode>(_leaves[character.ID]);
        foreach (QuadTreeNode node in currentLeaves)
            if (!newLeaves.Contains(node))
                _leaves[character.ID].Remove(node);
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

        _characters.Add(character);

        if (_characters.Count <= _nCharactersThreshold) //Node isn't overfilled
        {
            if (!_leaves.ContainsKey(character.ID))
                _leaves.Add(character.ID, new HashSet<QuadTreeNode>());
            _leaves[character.ID].Add(this);
            nodes.Add(this);
            return nodes;
        }

        if (_IsLeave)
        {
            //Leave becomes overfilled, so we create and fill its two children
            HashSet<CharacterInfo> leftCharacters = new HashSet<CharacterInfo>();
            HashSet<CharacterInfo> rightCharacters = new HashSet<CharacterInfo>();

            if (depth % 2 == 0) //Discriminate via x
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

            _leftChild = new QuadTreeNode($"{d_name}g", leftCharacters);
            _rightChild = new QuadTreeNode($"{d_name}d", rightCharacters);
        }

        if (!_IsLeave)
        {
            //Continue to run through the tree
            float depthMultiplicator = 1 / Mathf.Pow(2, depth / 2 + 1);

            if (depth % 2 == 0) //Discriminate via x
            {
                GizmosManager.toDrawStart.Add(new Vector3(x - x0, -y0));
                GizmosManager.toDrawEnd.Add(new Vector3(x - x0, y0));
                if (character.xMin < x)
                    nodes.UnionWith(_leftChild.RPlace(character, x - x0 * depthMultiplicator, y, depth + 1));
                if (character.xMax > x)
                    nodes.UnionWith(_rightChild.RPlace(character, x + x0 * depthMultiplicator, y, depth + 1));
            }
            else //Discriminate via y
            {
                GizmosManager.toDrawStart.Add(new Vector3(-x0, y - y0));
                GizmosManager.toDrawEnd.Add(new Vector3(x0, y - y0));
                if (character.yMin < y)
                    nodes.UnionWith(_leftChild.RPlace(character, x, y - y0 * depthMultiplicator, depth + 1));
                if (character.yMax > y)
                    nodes.UnionWith(_rightChild.RPlace(character, x, y + y0 * depthMultiplicator, depth + 1));
            }
        }
        return nodes;
    }
}
