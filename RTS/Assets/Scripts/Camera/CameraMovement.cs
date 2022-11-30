using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    private CameraControls _cameraActions;
    private InputAction _movement;
    private Camera _camera;
    private Transform _cameraTransform;

    [Header("Horizontal Translation")]
    [SerializeField]
    private float _max_speed = 5f;

    private float _speed;

    [Header("Horizontal Translation")]
    [SerializeField]
    private float _acceleration = 10f;

    [Header("Horizontal Translation")]
    [SerializeField]
    private float _damping = 15f;

    [Header("Vertical Translation")]
    [SerializeField]
    private float _stepSize = 2f;

    [Header("Vertical Translation")]
    [SerializeField]
    private float _zoomDampening = 7.5f;

    [Header("Vertical Translation")]
    [SerializeField]
    private float _minHeight = 5f;

    [Header("Vertical Translation")]
    [SerializeField]
    private float _maxHeight = 50f;

    [Header("Vertical Translation")]

    [Header("Rotation")]
    [SerializeField]
    private float _maxRotation_speed = 1f;

    [Header("Edge _movement")]
    [SerializeField]
    [Range(0f, 0.1f)]
    private float _edgeTolerance = 0.05f;

    //value set in various functions 
    //used to update the position of the camera base object.
    private Vector3 _targetPosition;

    private float _zoomHeight;

    //used to track and maintain velocity w/o a rigidbody
    private Vector3 _horizontalVelocity;
    private Vector3 _lastPosition;

    //tracks where the dragging action started
    private Vector3 _startDrag;

    private static CameraMovement _instance;

    private void Awake()
    {
        _instance = this;

        _cameraActions = new CameraControls();
        _camera = this.GetComponentInChildren<Camera>();
        _cameraTransform = _camera.transform;
    }

    private void OnEnable()
    {
        _zoomHeight = _camera.orthographicSize;

        _lastPosition = transform.position;

        _movement = _cameraActions.Camera.Move;
        _cameraActions.Camera.RotateCamera.performed += RotateCamera;
        _cameraActions.Camera.ZoomCamera.performed += ZoomCamera;
        _cameraActions.Camera.Enable();
    }

    private void OnDisable()
    {
        _cameraActions.Camera.RotateCamera.performed -= RotateCamera;
        _cameraActions.Camera.ZoomCamera.performed -= ZoomCamera;
        _cameraActions.Camera.Disable();
    }

    private void Update()
    {
        //inputs
        GetKeyboard_movement();
        CheckMouseAtScreenEdge();
        DragCamera();

        //move base and camera objects
        UpdateVelocity();
        UpdateBasePosition();
        UpdateCameraPosition();
    }

    public static void SetPosition(Vector2 position)
    {
        _instance._lastPosition = _instance.transform.position = position;
    }

    private void UpdateVelocity()
    {
        _horizontalVelocity = (transform.position - _lastPosition) / Time.deltaTime;
        _horizontalVelocity.y = 0f;
        _lastPosition = transform.position;
    }

    private void GetKeyboard_movement()
    {
        Vector3 inputValue = _movement.ReadValue<Vector2>().x * GetCameraRight()
                    + _movement.ReadValue<Vector2>().y * GetCameraForward();

        inputValue = inputValue.normalized;

        if (inputValue.sqrMagnitude > 0.1f)
            _targetPosition += inputValue;
    }

    private void DragCamera()
    {
        if (!Mouse.current.rightButton.isPressed)
            return;

        //create plane to raycast to
        Plane plane = new(-Vector3.forward, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (plane.Raycast(ray, out float distance))
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
                _startDrag = ray.GetPoint(distance);
            else
                _targetPosition += _startDrag - ray.GetPoint(distance);
        }
    }

    private void CheckMouseAtScreenEdge()
    {
        if (!Input.GetMouseButton(4) && !Input.GetKey(KeyCode.C))
            return;

        //mouse position is in pixels
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 moveDirection = Vector3.zero;

        //horizontal scrolling
        if (mousePosition.x < _edgeTolerance * Screen.width)
            moveDirection += -GetCameraRight();
        else if (mousePosition.x > (1f - _edgeTolerance) * Screen.width)
            moveDirection += GetCameraRight();

        //vertical scrolling
        if (mousePosition.y < _edgeTolerance * Screen.height)
            moveDirection += -GetCameraForward();
        else if (mousePosition.y > (1f - _edgeTolerance) * Screen.height)
            moveDirection += GetCameraForward();

        _targetPosition += moveDirection;
    }

    private void UpdateBasePosition()
    {
        if (_targetPosition.sqrMagnitude > 0.1f)
        {
            //create a ramp up or _acceleration
            _speed = Mathf.Lerp(_speed, _max_speed, Time.deltaTime * _acceleration);
            transform.position += _speed * Time.deltaTime * _targetPosition;
        }
        else
        {
            //create smooth slow down
            _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, Vector3.zero, Time.deltaTime * _damping);
            transform.position += _horizontalVelocity * Time.deltaTime;
        }

        //reset for next frame
        _targetPosition = Vector3.zero;
    }

    private void ZoomCamera(InputAction.CallbackContext obj)
    {
        float inputValue = -obj.ReadValue<Vector2>().y / 100f;

        if (Mathf.Abs(inputValue) > 0.1f)
        {
            _zoomHeight = _camera.orthographicSize + inputValue * _stepSize;

            if (_zoomHeight < _minHeight)
                _zoomHeight = _minHeight;
            else if (_zoomHeight > _maxHeight)
                _zoomHeight = _maxHeight;
        }
    }

    private void UpdateCameraPosition()
    {
        //set zoom target
        float zoomTarget = _zoomHeight;


        _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, zoomTarget, Time.deltaTime * _zoomDampening);
    }

    private void RotateCamera(InputAction.CallbackContext obj)
    {
        if (!Mouse.current.middleButton.isPressed)
            return;

        float inputValue = obj.ReadValue<Vector2>().x;
        transform.rotation = Quaternion.Euler(0f,0f, inputValue * _maxRotation_speed + transform.rotation.eulerAngles.z);
    }

    //gets the horizontal forward vector of the camera
    private Vector3 GetCameraForward()
    {
        Vector3 forward = _cameraTransform.up;
        forward.z = 0f;
        return forward;
    }

    //gets the horizontal right vector of the camera
    private Vector3 GetCameraRight()
    {
        Vector3 right = _cameraTransform.right;
        right.z = 0f;
        return right;
    }
}
