using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class ActiveBuilding : RBuilding
{
    protected abstract void PerformAction();
}
