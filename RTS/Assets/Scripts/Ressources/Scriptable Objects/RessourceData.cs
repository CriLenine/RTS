using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RessourceData : ScriptableObject
{
    [SerializeField]
    private RessourceType _type;

    [SerializeField]
    private int _amount;

    [SerializeField]
    private Vector2Int _position;

    public RessourceType Type => _type;

    public int Amount => _amount;

    public Vector2Int Position => _position;
}
