using System;
using UnityEngine;
using MyBox;
using System.Collections.Generic;

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
    [Separator("Sounds")]
    [SerializeField]
    private AudioClip _onSpawnAudio;
    [SerializeField]
    private List<AudioClip> _onSelectionAudio;
}
