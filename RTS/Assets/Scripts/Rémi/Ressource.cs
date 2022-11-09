using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ressource : MonoBehaviour
{
    public enum RessourceType
    {
        Coins,
        Plutonium,
        Water
    }

    [SerializeField]
    private RessourceData _data;
}
