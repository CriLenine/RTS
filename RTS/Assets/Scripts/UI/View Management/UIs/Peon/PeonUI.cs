using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PeonUtils
{
    Builds,
    Die
}

public class PeonUI : ViewManager
{
    public override void Initialize()
    {}

    public void OnDepth0Click(int index)
    {
        switch ((PeonUtils)index)
        {
            case PeonUtils.Builds:
                Show<ConstructionView>(true);
                break;
            case PeonUtils.Die:
                break;
        }
    }


}
