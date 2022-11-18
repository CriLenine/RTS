using UnityEngine;

public class BuildingBlueprintsManager : MonoBehaviour
{
    private static BuildingBlueprintsManager _instance;
    private Blueprint _actualBlueprint = null;

    private void Awake()
    {
        if( _instance == null )
            _instance = this;
        else
            Destroy( _instance );
    }

    public static void SpawnBlueprint(Building.Type building)
    {
        _instance._actualBlueprint ??= Blueprint.InstantiateWorldPos(building);
    }
}
