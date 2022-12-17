using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDStats : HUD
{
    [Header("Text Fields")]
    [Space]

    [SerializeField]
    private TextMeshProUGUI _name;
    [SerializeField]
    private TextMeshProUGUI _desc, _hp, _attackDamage, _attackRange, _meleeArmor, _rangeArmor, _resourceCollected;

    [Space]
    [Space]

    [Header("Icons")]
    [Space]

    [SerializeField]
    private Image _type;
    [SerializeField]
    private Image _weapon;
    [SerializeField]
    private Image _resource;

    [SerializeField]
    private Sprite _economyIconSprite, _militaryIconSprite;

    [Space]
    [Space]

    [Header("Misc")]
    [Space]

    [SerializeField]
    private Slider _healthBar;

    [SerializeField]
    private GameObject _bottomSeparator;

    private Character _character;
    private Building _building;

    public void DisplayStats(Character character)
    {
        Show();

        _character = character;
        _building = null;

        CharacterData data = character.Data;

        _name.text = character.CharaType.ToString();
        _desc.text = character is Peon ? "Economy Unit" : "Military Unit";
        _type.sprite = character is Peon ? _economyIconSprite : _militaryIconSprite;
        _attackDamage.text = $"{data.AttackDamage}";
        _attackRange.text = $"{data.AttackRange}";
        _meleeArmor.text = $"{data.MeleeArmor}";
        _rangeArmor.text = $"{data.RangeArmor}";

        _weapon.sprite = character.Data.Weapon;

        if (character is Peon)
        {
            Peon peon = character as Peon;

            if (peon.CarriedResource.Value > 0)
            {
                _bottomSeparator.SetActive(true);
                _resourceCollected.text = $"{peon.CarriedResource.Value}";
                _resource.gameObject.SetActive(true);
                return;
            }
        }

        _bottomSeparator.SetActive(false);
        _resource.gameObject.SetActive(false);
        _resourceCollected.text = string.Empty;
    }

    public void DisplayStats(Building building)
    {
        Show();

        _building = building;
        _character = null;

        BuildingData data = _building.Data;
        _name.text = _building.Data.Type.ToString();
        _desc.text = _building.BuildingType.ToString() == "Barracks" ? "Military Building" : "Economy Building";
        _type.sprite = _building.BuildingType.ToString() == "Barracks" ? _militaryIconSprite : _economyIconSprite;
        _attackDamage.text = "0";
        _attackRange.text = "0";
        _meleeArmor.text = $"{data.MeleeArmor}";
        _rangeArmor.text = $"{data.RangeArmor}";

        _bottomSeparator.SetActive(false);
        _resource.gameObject.SetActive(false);
        _resourceCollected.text = string.Empty;
    }

    private void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            if (_character != null)
            {
                float healthLossPercentage = 1 - (float)_character.CurrentHealth / _character.Data.MaxHealth;
                _healthBar.value = 1 - healthLossPercentage;

                _hp.text = $"{_character.CurrentHealth}/{_character.Data.MaxHealth}";
            }
            else if(_building != null)
            {
                float healthLossPercentage = 1 - (float)_building.CurrentHealth / _building.Data.MaxHealth;
                _healthBar.value = 1 - healthLossPercentage;

                _hp.text = $"{_building.CurrentHealth}/{_building.Data.MaxHealth}";
            }
        }
    }
}
