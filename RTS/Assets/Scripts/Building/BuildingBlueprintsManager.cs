using UnityEngine;

public class BuildingBlueprintsManager : MonoBehaviour
{
    private static BuildingBlueprintsManager _instance;
    private Blueprint _actualBlueprint = null;

    private void Awake()
    {
        _instance = this;
    }

    public static void SpawnBlueprint(Building.Type building)
    {
        _instance._actualBlueprint ??= Blueprint.InstantiateWorldPos(building);
    }
}
