using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDStats : HUD
{
    private static HUDStats _instance;

    [SerializeField]
    private TextMeshProUGUI _name, _hp, _attackDamage, _attackRange, _meleeArmor, _rangeArmor, _resourceCollected;

    [SerializeField]
    private Slider _healthBar;

    [SerializeField]
    private Image _weapon, _resource;

    private Character _character;

    protected void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this);
    }

    public void DisplayStats(Character character)
    {
        Show();

        _character = character;

        CharacterData data = character.Data;

        _name.text = character.Data.UnitName;
        _attackDamage.text = $"{data.AttackDamage}";
        _attackRange.text = $"{data.AutoAttackDistance}";
        _meleeArmor.text = $"{data.MeleeArmor}";
        _rangeArmor.text = $"{data.RangeArmor}";

        _weapon.sprite = character.Data.Weapon;

        if (character is Peon)
        {
            Peon peon = character as Peon;

            if (peon.CarriedResource.Value > 0)
            {
                _resourceCollected.text = $"{peon.CarriedResource.Value}";
                _resource.gameObject.SetActive(true);
                return;
            }
        }

        _resource.gameObject.SetActive(false);
        _resourceCollected.text = string.Empty;
    }

    private void Update()
    {
        if(gameObject.activeInHierarchy && _character != null)
        {
            float healthLossPercentage = 1 - (float)_character.CurrentHealth / _character.Data.MaxHealth;
            _healthBar.value = 1 - healthLossPercentage;

            _hp.text = $"{_character.CurrentHealth}/{_character.Data.MaxHealth}";
        }
    }
}
