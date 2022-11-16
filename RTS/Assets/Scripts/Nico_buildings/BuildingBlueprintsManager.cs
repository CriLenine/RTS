using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingBlueprintsManager : MonoBehaviour
{
    private static BuildingBlueprintsManager _instance;
    private Blueprint _actualBlueprint = null;


    private void Awake()
    {
        _instance = this;
    }
    public static void SpawnBlueprint(PeonBuilds building)
    {
        if(!_instance._actualBlueprint)
            _instance._actualBlueprint = Blueprint.InstantiateWorldPos(building);
    }
}
