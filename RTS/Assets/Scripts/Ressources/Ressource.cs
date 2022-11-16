using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RessourceType
{
    Coins,
    Plutonium,
    Water
}
public abstract class Ressource : TickedBehaviour
{

    [SerializeField]
    private RessourceData _data;

    public RessourceData Data => _data;

    public abstract Vector2Int GetHarvestingPosition(Vector2Int clickPosition);
}
