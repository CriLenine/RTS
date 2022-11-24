using System.Collections.Generic;
using UnityEngine;

public abstract class View : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UtilsView _type;
    public UtilsView Type => _type;

    [SerializeField] private Sprite _viewIcon;
    public Sprite ViewIcon => _viewIcon;



    public abstract void Initialize(ViewManager parentManager);

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }
    
}
