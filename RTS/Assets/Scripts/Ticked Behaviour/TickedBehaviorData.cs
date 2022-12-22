using System;
using UnityEngine;


public enum SubType
{
    Economy,
    Military
}

[Serializable]
public struct ButtonDataHUDParameters
{
    public ButtonData ButtonData;
    public Vector2Int ButtonPosition;
}

public abstract class TickedBehaviorData : ButtonData
{
}
