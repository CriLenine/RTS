using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildsManager : MonoBehaviour
{
    [SerializeField] private List<Building> _buildings;
    private GameObject _actualBlueprint = null;

    private Dictionary<PeonBuilds, Building> _buildingsPairs;
    private void Start()
    {
        _buildingsPairs = new Dictionary<PeonBuilds, Building>();

        foreach(var elem in _buildings)
        {
            _buildingsPairs.Add(elem.BuildingName, elem);
        }
        
    }
    public void SpawnBlueprint(PeonBuilds building)
    {
        Instantiate(_buildingsPairs[building].BluePrint,Vector2.zero, Quaternion.identity);
    }
}
