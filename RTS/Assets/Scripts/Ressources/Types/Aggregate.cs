using System.Collections.Generic;
using UnityEngine;

public class Aggregate : Resource
{
    [SerializeField]
    private List<Vector2Int> _rocks;

    #region DEBUG
    public RessourcesManager ressourcesManager;
    #endregion

    public void AddRock(Vector2Int newRock) => _rocks.Add(newRock);

    public override void Clear()
    {
        _rocks?.Clear();
    }

    public override Vector2Int GetTileToHarvest(Vector2Int coords)
    {
        for (int i = -1; i < 2; ++i)
        {
            for (int j = -1; j < 2; ++j)
            {
                if (_rocks.Contains(coords + new Vector2Int(i, j)))
                    return coords + new Vector2Int(i, j);
            }
        }
        Debug.LogError("No tile found to harvest");
        return coords;
    }

    private void Start()
    {
        ressourcesManager = FindObjectOfType<RessourcesManager>();
    }
}
