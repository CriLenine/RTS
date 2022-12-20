using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    [SerializeField]
    private Image _image;

    private Button _button;

    private ToolTipElement toolTipElement = null;

    private BuildingData _data;

    public void SetupButton(BuildingData data)
    {
        _data = data;

        _button = gameObject.AddComponent<Button>();
        _button.onClick.AddListener(() => Blueprint.SetActiveBlueprint(data));

        _image.color = data.Color;
        _image.sprite = data.HUDIcon;

        if (data.ToolTip != null)
        {
            toolTipElement = gameObject.AddComponent<ToolTipElement>();
            toolTipElement.Init(data.ToolTip);
        }
    }

    private void Update()
    {
        _button.interactable = TestInteractability();
    }

    private bool TestInteractability()
    {
        foreach (Resource.Amount cost in _data.Cost)
            if (GameManager.PlayerResources[cost.Type][NetworkManager.Me] < cost.Value)
                return false;
        return true;
    }

}
