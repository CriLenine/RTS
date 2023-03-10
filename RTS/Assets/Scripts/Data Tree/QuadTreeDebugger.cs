using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class QuadTreeDebugger : MonoBehaviour
{
    public static List<Vector3> toDrawStart = new List<Vector3>();
    public static List<Vector3> toDrawEnd = new List<Vector3>();

    private bool _debug = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6))
            _debug = !_debug;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !Application.isEditor)
            return;

        if (!_debug)
            return;

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
