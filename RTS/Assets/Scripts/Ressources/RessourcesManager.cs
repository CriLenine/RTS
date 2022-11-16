using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RessourcesManager : MonoBehaviour
{


    [SerializeField]
    private List<ForestRessource> _forests;

    private List<Vector2Int> _allTrees;

    [ContextMenu("Bake")]
    public void Bake()
    {
        //_allTrees = TileMapManager.GetTreesPositions();
        foreach (Vector2Int tree in _allTrees)
            FindNearestForest(tree).AddTree(tree);

        foreach (ForestRessource forest in _forests)
            forest.Bake();
    }

    private ForestRessource FindNearestForest(Vector2Int position)
    {
        (int minMagnitude, int index) = ((_forests[0].Data.Position - position).sqrMagnitude, 0);
        for (int i = 1; i < _forests.Count; i++)
        {
            int currentMagnitude = (_forests[i].Data.Position - position).sqrMagnitude;
            if (currentMagnitude < minMagnitude)
                (minMagnitude, index) = (currentMagnitude, i);
        }
        return _forests[index];
    }
}
