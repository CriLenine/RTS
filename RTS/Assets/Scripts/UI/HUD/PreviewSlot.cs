using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MyBox;

public class PreviewSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Separator("First Slot Data")]
    [Space]

    [SerializeField]
    private bool _isFirstSlot;

    [ConditionalField(nameof(_isFirstSlot))]
    public Image Fill;

    [Separator("Default Slot Data")]
    [Space]

    public Image PreviewSprite;
    public Button Cancel;

    private int _currentKey;
    public int CurrentKey { 
        get => _currentKey;  
        set => _currentKey = value; 
    }

    public void SetActive()
    {
        gameObject.SetActive(true);
        Cancel.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Cancel.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cancel.gameObject.SetActive(false);
    }

    public void CancelSpawn()
    {
        SelectionManager.SelectedBuilding.CancelSpawn(_currentKey);
    }
}
