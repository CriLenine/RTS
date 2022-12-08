using System.Collections.Generic;
using UnityEngine;

public abstract class View : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UtilsView _type;
    public UtilsView Type => _type;

    [SerializeField] private Sprite _viewIcon;
    public Sprite ViewIcon => _viewIcon;

    protected bool _blockClick = false;
    public bool Blocklick => _blockClick;

    public abstract void Initialize<T>(T parentManager) where T : ViewManager;

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

}
