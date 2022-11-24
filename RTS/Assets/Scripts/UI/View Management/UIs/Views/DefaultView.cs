using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultView : View
{
    private Dictionary<UtilsView,ItemUI> _utilViews;

    [SerializeField] private ItemUI _itemUI;
    [SerializeField] private Transform _itemParent;

    private ViewManager manager;
    public override void Initialize(ViewManager parentManager)
    {
        manager = parentManager;
        _utilViews = new Dictionary<UtilsView,ItemUI>();

        foreach (var view in manager.Views)
        {
            if (view == this) continue;

            var bUI = Instantiate(_itemUI);
            bUI.transform.SetParent(_itemParent);
            bUI.InitUI(OnClick, view.ViewIcon, view.Type.ToString(),view);

            _utilViews.Add(view.Type, bUI);
            bUI.gameObject.SetActive(false);
        }
    }

    public override void Show()
    {
        base.Show();

        foreach(var view in manager.ViewsType)
        {
            _utilViews[view].gameObject.SetActive(true);
        }
    }

    private void OnClick(View view)
    {
        manager.Show(view);
    }
}
