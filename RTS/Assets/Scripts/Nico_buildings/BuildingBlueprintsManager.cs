using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingBlueprintsManager : MonoBehaviour
{
    private Blueprint _actualBlueprint = null;

    public void SpawnBlueprint(PeonBuilds building)
    {
        _actualBlueprint = Blueprint.InstantiateWorldPos(building);
    }
}
