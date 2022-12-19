using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;

public class ToolTipVisual : MonoBehaviour
{
    [ConstantsSelection(typeof(ToolTipVisualTypes))]
    [SerializeField]
    private string _type = "Default";

    [Separator("Fields")]
    [Space]

    public TextMeshProUGUI Name;

    [Space]

    [ConditionalField(nameof(_type), false, ToolTipVisualTypes.Stat, ToolTipVisualTypes.Action, ToolTipVisualTypes.Building, ToolTipVisualTypes.Spawn)]
    public TextMeshProUGUI Description;

    [Space]
    [Space]

    [ConditionalField(nameof(_type), false, ToolTipVisualTypes.Building, ToolTipVisualTypes.Spawn)]
    public TextMeshProUGUI SubType;

    [Space]

    [ConditionalField(nameof(_type), false, ToolTipVisualTypes.Building, ToolTipVisualTypes.Spawn)]
    public Image Icon;

    [Space]

    [ConditionalField(nameof(_type), false, ToolTipVisualTypes.Building, ToolTipVisualTypes.Spawn)]
    public TextMeshProUGUICollection ResourceCostTexts;

    [ConditionalField(nameof(_type), false, ToolTipVisualTypes.Building, ToolTipVisualTypes.Spawn)]
    public ImageCollection ResourceCostIcons;

    [Space]

    [ConditionalField(nameof(_type), false, ToolTipVisualTypes.Spawn)]
    public TextMeshProUGUI TimeCost;
}

[Serializable]
public class TextMeshProUGUICollection : CollectionWrapper<TextMeshProUGUI> { }

[Serializable]
public class ImageCollection : CollectionWrapper<Image> { }

public class ToolTipVisualTypes
{
    public const string Default = "Default";
    public const string Stat = "Stat";
    public const string Action = "Action";
    public const string Building = "Building";
    public const string Spawn = "Spawn";
}