using UnityEngine;

public class Unit : MonoBehaviour
{
    private void Start()
    {
        TileMapManager.AddObstacleTile(gameObject.transform.position);
        UnitSelectionManager.AddUnit(gameObject);
    }

    void OnDestroy()
    {
        UnitSelectionManager.RemoveUnit(gameObject);
    }
}
