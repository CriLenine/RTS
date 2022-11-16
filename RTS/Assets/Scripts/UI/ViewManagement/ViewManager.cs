using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class ViewManager : MonoBehaviour
{
    private static ViewManager _instance;

    [SerializeField] private View _startingView;
    [SerializeField] private View[] _views;

    private View _currentView;

    private readonly Stack<View> _history = new();
    private void Awake() => _instance = this;

    public static T GetView<T>() where T : View
    {
        for (int i = 0; i < _instance._views.Length; i++)
        {
            if (_instance._views[i] is T tView)
            {
                return tView;
            }
        }

        return null;
    }

    public static T Show<T>(bool remember = true) where T : View
    {
        for (int i = 0; i < _instance._views.Length; i++)
        {
            if (_instance._views[i] is T)
            {
                if (_instance._currentView != null)
                {
                    if (remember)
                    {
                        _instance._history.Push(_instance._currentView);
                    }

                    _instance._currentView.Hide();
                }

                _instance._views[i].Show();

                _instance._currentView = _instance._views[i];

                return _instance._views[i] as T;
            }
        }
        Debug.Log("view do not exist");
        return null;
    }

    public static void Show(View view, bool remember = true)
    {
        if (_instance._currentView != null)
        {
            if (remember)
            {
                _instance._history.Push(_instance._currentView);
            }

            _instance._currentView.Hide();
        }

        view.Show();

        _instance._currentView = view;
    }


    public static void ShowLast()
    {
        if (_instance._history.Count != 0)
        {
            Show(_instance._history.Pop(), false);
        }
    }

    private void Start()
    {
        for (int i = 0; i < _views.Length; i++)
        {
            _views[i].Initialize();
            _views[i].Hide();
        }

        if (_startingView != null)
            Show(_startingView, true);
    }
}
