using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ViewManager : MonoBehaviour
{
    [SerializeField] 
    private View[] _views;

    [SerializeField]
    private TickedBehaviour _owner;
    public TickedBehaviour Owner => _owner;

    private View _currentView=null;

    private readonly Stack<View> _history = new();

    private void Awake()
    {
        for (int i = 0; i < _views.Length; i++)
        {
            _views[i].Initialize();
            _views[i].Hide();
        }

    }
    private void OnEnable()
    {
        for (int i = 0; i < _views.Length; i++)
        {

            _views[i].Hide();
        }

        Show<DefaultView>();
    }
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

    public abstract void Initialize();
    public virtual void Hide() => gameObject.SetActive(false);
    public virtual void Show() => gameObject.SetActive(true);

}
