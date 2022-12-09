using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ToolTipManager : MonoBehaviour
{
    private static ToolTipManager _instance;

    [SerializeField]
    private ToolTipVisual _defaultToolTipVisual;

    private bool _displayed = false;

    protected void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this);
    }

    public static void DisplayDefaultToolTip(ToolTip toolTip)
    {
        _instance._displayed = true;
        _instance._defaultToolTipVisual.Visual.SetActive(true);
        _instance._defaultToolTipVisual.Name.text = toolTip.Name;
    }

    public static void HideToolTip()
    {
        _instance._defaultToolTipVisual.Visual.SetActive(false);
    }

    private void Update()
    {
        if (_instance._displayed)
            _instance._defaultToolTipVisual.Visual.transform.position = Mouse.current.position.ReadValue();
    }
}
