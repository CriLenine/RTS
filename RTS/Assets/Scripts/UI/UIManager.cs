using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;

    [Header("Views")]
    [SerializeField]
    private ViewManager[] _tickedBehaviourUI;

    private Dictionary<Type,ViewManager> _viewManagers;

    private ViewManager _currentViewManager;

    public static ViewManager CurrentManager => _instance._currentViewManager;
    protected void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
        {
            Destroy(this);
            return;
        }
        _viewManagers = new Dictionary<Type, ViewManager>();

    }
    private void Start()
    {

        foreach (var tb in _tickedBehaviourUI)
        {
            _viewManagers.Add(tb.Owner, tb);
            tb.Initialize();
            tb.HideUI();
        }
    }


    public static void ShowTickedBehaviourUI<T>(T owner) where T : TickedBehaviour
    {
        if (!_instance._viewManagers.TryGetValue(typeof(T), out ViewManager viewManager))
        {
            throw new NotImplementedException("Missing viewManager");
        }

        HideCurrentUI();

        viewManager.ShowUI(owner);
        _instance._currentViewManager = viewManager;

    }

    public static void HideCurrentUI()
    {
        if (_instance._currentViewManager != null)
        {
            _instance._currentViewManager.HideUI();
            _instance._currentViewManager = null;
        }
    }

}
