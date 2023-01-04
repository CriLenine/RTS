using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;

public class ToolTipVisual : MonoBehaviour
{
    [SerializeField]
    private ToolTipVisualType _type;

    [Separator("Fields")]
    [Space]

    public TextMeshProUGUI Name;

    [Space]

    [ConditionalField(nameof(_type), false, ToolTipVisualType.Stat, ToolTipVisualType.Action, ToolTipVisualType.Building, ToolTipVisualType.Spawn)]
    public TextMeshProUGUI Description;

    [Space]
    [Space]

    [ConditionalField(nameof(_type), false, ToolTipVisualType.Building, ToolTipVisualType.Spawn)]
    public TextMeshProUGUI SubType;

    [Space]

    [ConditionalField(nameof(_type), false, ToolTipVisualType.Building, ToolTipVisualType.Spawn)]
    public Image Icon;

    [Space]

    [ConditionalField(nameof(_type), false, ToolTipVisualType.Building, ToolTipVisualType.Spawn)]
    public TextMeshProUGUICollection ResourceCostTexts;

    [ConditionalField(nameof(_type), false, ToolTipVisualType.Building, ToolTipVisualType.Spawn)]
    public ImageCollection ResourceCostIcons;

    [Space]

    [ConditionalField(nameof(_type), false, ToolTipVisualType.Spawn)]
    public TextMeshProUGUI TimeCost;
}

[Serializable]
public class TextMeshProUGUICollection : CollectionWrapper<TextMeshProUGUI> { }

[Serializable]
public class ImageCollection : CollectionWrapper<Image> { }

public enum ToolTipVisualType
{
    Default,
    Stat,
    Action,
    Building,
    Spawn
}