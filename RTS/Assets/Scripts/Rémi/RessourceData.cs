using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RessourceData : MonoBehaviour
{
    [SerializeField]
    private Ressource.RessourceType _type;

    [SerializeField]
    private int _amount;

    public Ressource.RessourceType Type => _type;

    public int Amount => _amount;
}
