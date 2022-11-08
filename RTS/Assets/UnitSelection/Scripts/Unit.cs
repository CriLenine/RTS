using UnityEngine;

public class Unit : MonoBehaviour
{
    private void Start()
    {
        UnitSelectionManager.AddUnit(this.gameObject);
    }

    void OnDestroy()
    {
        UnitSelectionManager.RemoveUnit(this.gameObject);
    }
}
