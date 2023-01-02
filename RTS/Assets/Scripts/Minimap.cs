using MyBox;
using UnityEngine;
using UnityEngine.EventSystems;

public class Minimap : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField]
    private Camera _minimapCamera;

    [SerializeField]
    private LineRenderer _frustrumRenderer;

    [SerializeField]
    private RectTransform _rectTransform;

    private Vector3[] _frustrumPoints = new Vector3[4];

    [SerializeField]
    private bool _usePercentageSize = true;

    [SerializeField]
    [ConditionalField(nameof(_usePercentageSize))]
    [Range(0f, 1f), Tooltip("Percentage of screen height")]
    private float _percentageSize = 0.45f;

    [SerializeField]
    [ConditionalField(nameof(_usePercentageSize), inverse: true)]
    [Min(0f)]
    private int _pixelSize = 250;

    [SerializeField]
    private Vector2Int _offset;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnMinimapInteraction(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnMinimapInteraction(eventData);
    }

    private void OnMinimapInteraction(PointerEventData eventData)
    {
        Vector3[] corners = new Vector3[4];

        _rectTransform.GetWorldCorners(corners);

        Bounds bounds = new Bounds(corners[0], Vector3.zero);

        for (int i = 1; i < 4; ++i)
            bounds.Encapsulate(corners[i]);

        float x = (eventData.position.x - bounds.min.x) / bounds.size.x;
        float y = (eventData.position.y - bounds.min.y) / bounds.size.y;

        CameraMovement.SetPosition(_minimapCamera.ViewportToWorldPoint(new Vector3(x, y)));
    }

    private void Update()
    {
        UpdateViewport();
    }

    private void UpdateViewport()
    {
        Camera camera = Camera.main;

        int screenWidth = camera.pixelWidth;
        int screenHeight = camera.pixelHeight;

        float size = _usePercentageSize ? _percentageSize * screenHeight : _pixelSize;

        float width = size / screenWidth;
        float height = size / screenHeight;

        float x = (screenWidth - size - _offset.x) / screenWidth;
        float y = (screenHeight - size - _offset.y) / screenHeight;

        _rectTransform.anchorMin = new Vector2(x, y);
        _rectTransform.anchorMax = _rectTransform.anchorMin + new Vector2(width, height);

        _frustrumRenderer.startWidth = _frustrumRenderer.endWidth = _minimapCamera.orthographicSize / 80f;

        _frustrumRenderer.loop = true;

        _frustrumPoints[0] = camera.ViewportToWorldPoint(Vector3.zero);
        _frustrumPoints[0].z = 0f;

        _frustrumPoints[1] = camera.ViewportToWorldPoint(Vector3.right);
        _frustrumPoints[1].z = 0f;

        _frustrumPoints[2] = camera.ViewportToWorldPoint(Vector3.one);
        _frustrumPoints[2].z = 0f;

        _frustrumPoints[3] = camera.ViewportToWorldPoint(Vector3.up);
        _frustrumPoints[3].z = 0f;

        _frustrumRenderer.SetPositions(_frustrumPoints);
    }

    private void OnValidate()
    {
        if (_frustrumRenderer is null || _rectTransform is null || _minimapCamera is null)
            return;

        UpdateViewport();
    }
}
