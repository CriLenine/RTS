using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionButton : MonoBehaviour
{
    [SerializeField]
    private Image _image;

    [SerializeField]
    private TextMeshProUGUI _count;

    private ToolTipElement _tooltipElement;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void SetupButton(Character.Type type)
    {
        CharacterData characterData = DataManager.GetCharacterData(type);

        _image.color = characterData.Color;
        _image.sprite = characterData.Icon;

        UpdateCount(type);

        if (_tooltipElement == null)
            _tooltipElement = gameObject.AddComponent<ToolTipElement>();

        _tooltipElement.Init(characterData.SelectionToolTip);

        _button.onClick.AddListener(() => SelectionManager.SpecializeSelection(type));

        _button.interactable = true;
    }

    public void SetActivePulse(bool value)
    {
        gameObject.GetComponent<Pulsation>().enabled = value;
    }

    public void UpdateCount(Character.Type type)
    {
        _count.gameObject.SetActive(true);
        _count.text = (type == Character.Type.All ? SelectionManager.AllSelectedCharactersCount : SelectionManager.Counts[type]).ToString();
    }

    public void ResetButton()
    {
        _image.sprite = null;
        _image.color = HUDManager.DefaultButtonColor;

        _count.gameObject.SetActive(false);

        if (_tooltipElement != null)
            Destroy(_tooltipElement);

        _button.interactable = false;
    }
}
