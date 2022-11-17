using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    private static BuildingManager _instance;

    private List<Building> _preConstructionBuild;
    private List<Building> _constructionBuild;
    private List<Building> _build;
    private void Awake()
    {
        _instance = this;

        _preConstructionBuild = new List<Building>();
        _constructionBuild = new List<Building>();
        _build = new List<Building>();
    }

    public static void StartBuild(LogicalTile position, List<Peon> builders)
    {
        if(_instance._preConstructionBuild.Count ==0)
        {
            Debug.Log("NoBuildingInPreconstruction");
            return;
        }
        foreach(var build in _instance._preConstructionBuild)
        {
            if (position.Coords == TileMapManager.WorldToTilemapCoords(build.transform.position))
            {
                foreach (var builder in builders)
                    builder.SetBuild(build);
            }
        }
    }

    public static void AddBuilding(Building build)
    {
        _instance._preConstructionBuild.Add(build);
    }

}
