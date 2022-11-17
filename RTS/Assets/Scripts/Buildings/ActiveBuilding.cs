using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class ActiveBuilding : Building
{
    protected abstract void PerformAction();
}
