using System;
using UnityEngine;
using MyBox;
using TMPro;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ToolTip", menuName = "ToolTip", order = 1)]

public class ToolTip : ScriptableObject
{
    public ToolTip(string name)
    {
        _type = ToolTipType.Default;
        _name = name;
    }

    [SerializeField]
    private ToolTipType _type;
    public ToolTipType Type => _type;

    [Space]
    [Separator("Fields")]
    [Space]

    [SerializeField]
    private string _name;
    public string Name => _name;

    [ConditionalField(nameof(_type), false, ToolTipType.Stat, ToolTipType.Action, ToolTipType.Building, ToolTipType.SpawnResearch)]

    [Space]

    [SerializeField]
    [TextArea(3, 10)]
    private string _description;
    public string Description => _description;

    [Space]
    [Space]

    [ConditionalField(nameof(_type), false, ToolTipType.Building)]

    [SerializeField]
    private BuildingData _buildingData;
    public BuildingData BuildingData => _buildingData;

    [ConditionalField(nameof(_type), false, ToolTipType.SpawnResearch)]

    [SerializeField]
    private CharacterData _data;
    public CharacterData Data => _data;
}
