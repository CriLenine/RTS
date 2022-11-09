using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PeonUtils
{
    Builds,
    Die
}
public enum PeonBuilds
{
    Farm,
    Barracks
}
public class PeonUiManager : MonoBehaviour
{
    [SerializeField] private GameObject _depth0;
    [SerializeField] private GameObject _buildings;
    [SerializeField] private BuildingBlueprintsManager _buildsManager;


    public void OnDepth0Click(int index)
    {
        switch((PeonUtils)index)
        {
            case PeonUtils.Builds:
                _depth0.SetActive(false);
                _buildings.SetActive(true);
                break;
            case PeonUtils.Die:
                break;
        }
    }

    public void OnBuildingClick(int index)
    {
        _buildsManager.SpawnBlueprint((PeonBuilds)index);
    }
}
