using UnityEngine;

public class Minimap : MonoBehaviour
{
    [SerializeField]
    private Camera _minimapCamera;

    [SerializeField]
    private Vector2Int _size;

    [SerializeField]
    private Vector2Int _offset;

    private void Awake()
    {
        
    }

    private void Start()
    {

    }

    private void Update()
    {
        float width = _size.x / (float)Screen.width;
        float height = _size.y / (float)Screen.height;

        _minimapCamera.rect = new Rect((Screen.width - _size.x - _offset.x) / (float)Screen.width, (Screen.height - _size.y - _offset.y) / (float)Screen.height, width, height);
    }
}
