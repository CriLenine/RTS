using UnityEngine;

public class FogOfWarManager : MonoBehaviour
{
    private static FogOfWarManager _instance;

    [SerializeField]
    private Camera _persistentCamera;

    [SerializeField]
    private Camera _currentCamera;

    [SerializeField]
    private RectTransform _canvas;

    void Awake()
    {
        _instance = this;
    }

    public static void ResetFog()
    {
        _instance._persistentCamera.targetTexture.Release();
        _instance._currentCamera.targetTexture.Release();
    }

    public static void UpdateFog()
    {
        _instance._persistentCamera.Render();
        _instance._currentCamera.Render();
    }

    public static void SetFogActive(bool active)
    {
        _instance._canvas.gameObject.SetActive(active);
    }
}
