using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum UiType
{
    Chara,
    Building
}

public enum UtilsView
{
    Default,
    Construction,
    Spawn,
    SetRallyPoint
}
public abstract class ViewManager : MonoBehaviour
{
    #region Manager UI
    [SerializeField] protected TextMeshProUGUI _title;
    #endregion

    #region Data fields
    protected UtilsView[] UtilViews;
    public UtilsView[] ViewsType=> UtilViews;
    #endregion

    [SerializeField]
    private UiType _owner;
    public Type Owner => _owner==0 ? typeof(Character): typeof(Building);

    //Viewmanagement
    [SerializeField]
    protected View[] _views;
    public View[] Views => _views;

    private View _currentView=null;

    private readonly Stack<View> _history = new();

    public abstract void Initialize();

    public T GetView<T>() where T : View
    {
        for (int i = 0; i < _views.Length; i++)
        {
            if (_views[i] is T tView)
            {
                return tView;
            }
        }

        return null;
    }

    public T Show<T>(bool remember = true) where T : View
    {
        for (int i = 0; i < _views.Length; i++)
        {
            if (_views[i] is T)
            {
                if (_currentView != null)
                {
                    if (remember)
                    {
                        _history.Push(_currentView);
                    }

                    _currentView.Hide();
                }

                _views[i].Show();

                _currentView = _views[i];

                return _views[i] as T;
            }
        }
        Debug.Log("view do not exist");
        return null;
    }

    public void Show(View view, bool remember = true)
    {
        if (_currentView != null)
        {
            if (remember)
            {
                _history.Push(_currentView);
            }

            _currentView.Hide();
        }

        view.Show();

        _currentView = view;
    }


    public void ShowLast()
    {
        if (_history.Count != 0)
        {
            Show(_history.Pop(), false);
        }
    }

    public virtual void HideUI() => gameObject.SetActive(false);
    public virtual void ShowUI<T>(T uiOwner) where T : TickedBehaviour
    {
        gameObject.SetActive(true);

        for (int i = 0; i < _views.Length; i++)
        {

            _views[i].Hide();
        }

        Show<DefaultView>();

    }

}
