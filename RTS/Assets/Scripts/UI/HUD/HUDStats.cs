using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;

public class HUDStats : HUD
{
    [Separator("Text Fields")]

    [SerializeField]
    private TextMeshProUGUI _name;
    [SerializeField]
    private TextMeshProUGUI _subType, _hp, _attackDamage, _attackRange, _meleeArmor, _rangeArmor, _resourceCollected;

    [Space]
    [Separator("Icons")]

    [SerializeField]
    private Image _icon;
    [SerializeField]
    private Image _weapon;
    [SerializeField]
    private Image _resource;

    [Space]
    [Separator("Misc")]

    [SerializeField]
    private Slider _healthBar;

    [SerializeField]
    private GameObject _bottomSeparator;

    [Space]
    [Separator("Colors")]

    [SerializeField]
    private Color _allyColor;

    [SerializeField]
    private Color _enemyColor;

    private Character _character;
    private Building _building;

    public void DisplayStats(Character character)
    {
        Show();

        _character = character;
        _building = null;

        CharacterData data = character.Data;

        _name.text = character.Data.Type.ToString();
        _name.color = character.Performer == NetworkManager.Me ? _allyColor : _enemyColor;

        if (character.Data.SubType == SubType.Economy)
        {
            _subType.text = "Economy Unit";
            _subType.color = HUDManager.EconomyTypeColor;
            _icon.sprite = HUDManager.EconomyTypeSprite;
            _icon.color = HUDManager.EconomyTypeColor;
        }
        else
        {
            _subType.text = "Military Unit";
            _subType.color = HUDManager.MilitaryTypeColor;
            _icon.sprite = HUDManager.MilitaryTypeSprite;
            _icon.color = HUDManager.MilitaryTypeColor;
        }

        _attackDamage.text = $"{data.AttackDamage}";
        _attackRange.text = $"{data.AttackRange}";
        _meleeArmor.text = $"{data.MeleeArmor}";
        _rangeArmor.text = $"{data.RangeArmor}";

        _weapon.sprite = character.Data.Weapon;

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
        _name.color = _building.Performer == NetworkManager.Me ? _allyColor : _enemyColor;

        if (building.Data.SubType == SubType.Economy)
        {
            _subType.text = "Economy Building";
            _subType.color = HUDManager.EconomyTypeColor;
            _icon.sprite = HUDManager.EconomyTypeSprite;
            _icon.color = HUDManager.EconomyTypeColor;
        }
        else
        {
            _subType.text = "Military Building";
            _subType.color = HUDManager.MilitaryTypeColor;
            _icon.sprite = HUDManager.MilitaryTypeSprite;
            _icon.color = HUDManager.MilitaryTypeColor;
        }

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

                if (_character.Data.CanHarvestResources)
                {
                    if (_character.HarvestedResource.Value > 0)
                    {
                        _bottomSeparator.SetActive(true);
                        _resourceCollected.text = $"{_character.HarvestedResource.Value}";
                        _resource.gameObject.SetActive(true);
                        return;
                    }
                }
            }
            else if (_building != null)
            {
                float healthLossPercentage = 1 - (float)_building.CurrentHealth / _building.Data.MaxHealth;
                _healthBar.value = 1 - healthLossPercentage;

                _hp.text = $"{_building.CurrentHealth}/{_building.Data.MaxHealth}";
            }
        }
    }
}
