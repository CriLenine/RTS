using UnityEngine;

[CreateAssetMenu(fileName = "Ressource", menuName = "Ressource", order = 1)]
public class RessourceData : ScriptableObject
{
    [SerializeField]
    private ResourceType _type;

    [SerializeField]
    private int _amount;

    [SerializeField]
    private float _harvestingTime;

    [SerializeField]
    private Sprite _sprite;

    public Sprite Sprite => _sprite;

    public ResourceType Type => _type;

    public int Amount { get => _amount; set { _amount = value; } }

    public float HarvestingTime => _harvestingTime;
}
