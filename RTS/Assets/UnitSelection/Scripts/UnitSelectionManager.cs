using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    private static UnitSelectionManager _instance;

    private Camera _camera;

    [SerializeField]
    private List<GameObject> _unitsList = new List<GameObject> ();

    [SerializeField]
    private List<GameObject> _unitsSelected = new List<GameObject>();

    private void Awake()
    {
       if( _instance != null && _instance != this)
            Destroy(this.gameObject);
       else 
            _instance = this;   
    }

    private void Start()
    {
        _camera = Camera.main;
    }

    public static void AddUnit(GameObject unitToAdd)
    {
        _instance._unitsList.Add(unitToAdd);
    }

    public static void RemoveUnit(GameObject unitToRemove)
    {
        _instance._unitsList.Remove(unitToRemove);
    }

    public static void ClickSelect(GameObject unitToAdd)
    {
        DeselectAll();
        _instance._unitsSelected.Add(unitToAdd);
        unitToAdd.transform.GetChild(1).gameObject.SetActive(true);
    }

    public static void ShiftClickSelect(GameObject unitToAdd)
    {
        if (!_instance._unitsSelected.Contains(unitToAdd))
        {
            _instance._unitsSelected.Add(unitToAdd);
            unitToAdd.transform.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            _instance._unitsSelected.Remove(unitToAdd);
            unitToAdd.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    public static void DragSelect(Rect selectionBox, bool shifting)
    {
        if (!shifting)
            DeselectAll();

        foreach (GameObject unit in _instance._unitsList)
            if (selectionBox.Contains(_instance._camera.WorldToScreenPoint(unit.transform.position)))
                if (!(shifting && _instance._unitsSelected.Contains(unit)))
                {
                    _instance._unitsSelected.Add(unit);
                    unit.transform.GetChild(1).gameObject.SetActive(true);
                }
    }

    public static void DeselectAll()
    {
        foreach(GameObject unitToRemove in _instance._unitsSelected)
            unitToRemove.transform.GetChild(1).gameObject.SetActive(false);
        _instance._unitsSelected.Clear();
    }
}
