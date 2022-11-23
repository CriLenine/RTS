using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GizmosManager : MonoBehaviour
{
    public static List<Vector3> toDrawStart = new List<Vector3>();
    public static List<Vector3> toDrawEnd = new List<Vector3>();
    private void OnDrawGizmos()
    {
        for (int i = 0; i < toDrawStart.Count; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(toDrawStart[i], toDrawEnd[i]);
        }
    }

    public static void Clear()
    {
        toDrawStart?.Clear();
        toDrawEnd?.Clear();
    }
}
