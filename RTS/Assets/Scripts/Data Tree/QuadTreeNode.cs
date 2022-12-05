using System.Collections.Generic;
using UnityEngine;

public class QuadTreeNode
{
    public static QuadTreeNode QuadTreeRoot;

    private static int _nCharactersThreshold;
    private static float x0, y0;
    private static Dictionary<int, HashSet<QuadTreeNode>> _leaves;
    private static Dictionary<int, (float width, float height)> _charactersHitBoxes;
    private static Dictionary<int, Vector2> _charactersPositions;

    private readonly HashSet<int> _characters;

    private QuadTreeNode _leftChild;
    private QuadTreeNode _rightChild;


    private bool _IsLeave => _leftChild == null;//Cannot have only 1 child so no need to check rightChild

    #region Debug
    public static Dictionary<int, HashSet<QuadTreeNode>> d_Leaves => _leaves;
    public readonly string d_name = "";
    public int d_NodesCount => 1 + (_IsLeave ? 0 : _leftChild.d_NodesCount + _rightChild.d_NodesCount);
    public int d_Depth => _IsLeave ? 0 : 1 + Mathf.Max(_leftChild.d_Depth, _rightChild.d_Depth);

    private float XMAX = x0;
    private float XMIN = -x0;
    private float YMAX = y0;
    private float YMIN = -y0;
    #endregion

    private QuadTreeNode(string d_name, float xmax, float xmin, float ymax, float ymin, HashSet<int> value = null, QuadTreeNode leftChild = null, QuadTreeNode rightChild = null)
    {
        _characters = value ?? new HashSet<int>();
        _leftChild = leftChild;
        _rightChild = rightChild;
        this.d_name = d_name;
        XMAX = xmax;
        XMIN = xmin;
        YMAX = ymax;
        YMIN = ymin;
    }

    /// <summary>
    /// Creates a first node (the root) and initializes the essential variables.
    /// </summary>
    /// <param name="nCharactersThreshold">The maximum of characters we want in the same node</param>
    /// <param name="x0">The x corresponding to the middle of the area</param>
    /// <param name="y0">The y corresponding to the middle of the area</param>
    public static void Init(int nCharactersThreshold, float x0, float y0)
    {
        _nCharactersThreshold = nCharactersThreshold;
        QuadTreeNode.x0 = x0;
        QuadTreeNode.y0 = y0;
        _leaves = new Dictionary<int, HashSet<QuadTreeNode>>();
        _charactersHitBoxes = new Dictionary<int, (float width, float height)>();
        _charactersPositions = new Dictionary<int, Vector2>();
        QuadTreeRoot = new QuadTreeNode("r", x0, -x0, y0, -y0);
    }

    /// <summary>
    /// Called at the creation of the character.
    /// Puts it in the root node
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="position"></param>
    public static void RegisterCharacter(int ID, float width, float height, Vector2 position)
    {
        _charactersHitBoxes.Add(ID, (width, height));
        _charactersPositions.Add(ID, position);
        QuadTreeRoot._characters.Add(ID);
        _leaves.Add(ID, new HashSet<QuadTreeNode>());
        _leaves[ID].Add(QuadTreeRoot);
    }

    /// <summary>
    /// Called at the destruction of an entitie
    /// </summary>
    /// <param name="characterID"></param>
    public static void RemoveCharacter(int characterID)
    {
        _charactersHitBoxes.Remove(characterID);
        _charactersPositions.Remove(characterID);
        _leaves.Remove(characterID);
    }

    /// <summary>
    /// Called when moving a character.
    /// <para>Updates the tree with the character's new position.</para>
    /// </summary>
    /// <param name="ID">The ID of the moved character.</param>
    /// <returns>All the IDs of characters in same leaves than the selected character.</returns>
    public static HashSet<int> GetNeighbours(int ID, Vector2 position, int nCharactersThreshold = 0)
    {
        if (!_charactersHitBoxes.ContainsKey(ID))
        {
            Debug.LogError("ID not registered.");
            return null;
        }
        _charactersPositions[ID] = position;
        if (nCharactersThreshold > 0)
            _nCharactersThreshold = nCharactersThreshold;
        return QuadTreeRoot.GetNeighbours(ID);
    }

    private HashSet<int> GetNeighbours(int ID)
    {
        UpdateTree(ID);
        HashSet<int> neighbours = new HashSet<int>();
        foreach (QuadTreeNode leave in _leaves[ID])
            foreach (int characterID in leave._characters)
                if (characterID != ID && _charactersHitBoxes.ContainsKey(characterID))
                    neighbours.Add(characterID);
        return neighbours;
    }
    /// <summary>
    /// Called in <see cref="GetNeighbours(CharacterInfo)"/>.
    /// <para>Places the character in its new correct leaves, then updates <see cref="_leaves"/>.</para>
    /// </summary>
    private void UpdateTree(int ID)
    {
        HashSet<QuadTreeNode> newLeaves = RPlace(ID, x0, y0);
        HashSet<QuadTreeNode> currentLeaves = new HashSet<QuadTreeNode>(_leaves[ID]);
        foreach (QuadTreeNode node in currentLeaves)
            if (!newLeaves.Contains(node))
            {
                node._characters.Remove(ID);
                _leaves[ID].Remove(node);
            }
    }

    /// <summary>
    /// Recursive.
    /// <para>Called on each tree update.</para>
    /// Runs through the tree to place the <paramref name="character"/> into the correct leaves, creating ones if necessary.
    /// </summary>
    /// <returns>A <see cref="HashSet{QuadTreeNode}"/> containing the leaves where the <paramref name="character"/> is stocked.</returns>
    private HashSet<QuadTreeNode> RPlace(int ID, float x, float y, int depth = 0)
    {
        HashSet<QuadTreeNode> nodes = new HashSet<QuadTreeNode>();

        _characters.Add(ID);

        if (_characters.Count <= _nCharactersThreshold) //Node isn't overfilled
        {
            _leaves[ID].Add(this);
            nodes.Add(this);
            return nodes;
        }

        if (_IsLeave)
        {
            //Leave becomes overfilled, so we create and fill its two children
            HashSet<int> leftCharacters = new HashSet<int>();
            HashSet<int> rightCharacters = new HashSet<int>();

            if (depth % 2 == 0) //Discriminate via x
            {
                foreach (int IDToSort in _characters)
                {
                    if (!_charactersHitBoxes.ContainsKey(IDToSort))
                        continue;
                    float xCurrent = _charactersPositions[IDToSort].x;
                    float width = _charactersHitBoxes[IDToSort].width;
                    float xMin = xCurrent - width / 2f;
                    float xMax = xCurrent + width / 2f;
                    if (xMin < x)
                        leftCharacters.Add(IDToSort);
                    if (xMax > x)
                        rightCharacters.Add(IDToSort);
                }
            }
            else //Discriminate via y
            {
                foreach (int IDToSort in _characters)
                {
                    if (!_charactersHitBoxes.ContainsKey(IDToSort))
                        continue;

                    float yCurrent = _charactersPositions[IDToSort].y;
                    float height = _charactersHitBoxes[IDToSort].height;
                    float yMin = yCurrent - height / 2f;
                    float yMax = yCurrent + height / 2f;
                    if (yMin < y)
                        leftCharacters.Add(IDToSort);
                    if (yMax > y)
                        rightCharacters.Add(IDToSort);
                }
            }

            _leftChild = new QuadTreeNode($"{d_name}g", XMAX, XMIN, YMAX, YMIN, leftCharacters);
            _rightChild = new QuadTreeNode($"{d_name}d", XMAX, XMIN, YMAX, YMIN, rightCharacters);
        }

        //Continue to run through the tree
        float depthMultiplicator = 1 / Mathf.Pow(2, depth / 2 + 1);

        if (depth % 2 == 0) //Discriminate via x
        {
            GizmosManager.toDrawStart.Add(new Vector3(x - x0, YMAX));
            GizmosManager.toDrawEnd.Add(new Vector3(x - x0, YMIN));

            float xCurrent = _charactersPositions[ID].x;
            float width = _charactersHitBoxes[ID].width;
            float xMin = xCurrent - width / 2f;
            float xMax = xCurrent + width / 2f;
            if (xMin < x)
                nodes.UnionWith(_leftChild.RPlace(ID, x - x0 * depthMultiplicator, y, depth + 1));
            if (xMax > x)
                nodes.UnionWith(_rightChild.RPlace(ID, x + x0 * depthMultiplicator, y, depth + 1));
        }
        else //Discriminate via y
        {
            GizmosManager.toDrawStart.Add(new Vector3(XMAX, y - y0));
            GizmosManager.toDrawEnd.Add(new Vector3(XMIN, y - y0));

            float yCurrent = _charactersPositions[ID].y;
            float height = _charactersHitBoxes[ID].height;
            float yMin = yCurrent - height / 2f;
            float yMax = yCurrent + height / 2f;
            if (yMin < y)
                nodes.UnionWith(_leftChild.RPlace(ID, x, y - y0 * depthMultiplicator, depth + 1));
            if (yMax > y)
                nodes.UnionWith(_rightChild.RPlace(ID, x, y + y0 * depthMultiplicator, depth + 1));
        }
        return nodes;
    }
}
