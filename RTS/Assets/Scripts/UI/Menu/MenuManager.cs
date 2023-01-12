using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;


public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private Image _blackScreen;

    [Space]

    [SerializeField]
    private TextMeshProUGUI _pressAnyKey;

    [SerializeField]
    private Transform _pressAnyKeyTransform, _loadingCircleTransform, _roomLayout;

    [Space]

    [SerializeField]
    private CanvasGroup _strip;
    [SerializeField]
    private CanvasGroup _roomSelectionBox, _settingsBox, _creditsBox, _quitBox;

    [Space]

    [SerializeField]
    private GameObject _roomTemplatePrefab;
    
    [Space]

    [SerializeField]
    private Button _joinRoomButton;

    [SerializeField]
    private MenuButton _joinRoomMenuButton;

    [SerializeField]
    private Color _defaultDisabledColor, _defaultColor;

    [Space]

    [SerializeField]
    private CanvasGroup _mainMenuButtons;
    [SerializeField]
    private CanvasGroup _gameCreationButtons, _roomSelectionButtons, _preGameButtons, _loading;

    private CanvasGroup _currentCanvas;

    private NetworkManager.Room[] _serverRooms;
    private MenuRoom _selectedRoom;

    private bool _awaitingInteraction = true;
    private bool _attemptingToConnect = false;
    private bool _attemptingToJoin = false;
    private bool _loadingRooms = false;
    private bool _roomsLoaded = false;

    private void Update()
    {
        if (_awaitingInteraction)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
            {
                _awaitingInteraction = false;

                _strip.gameObject.SetActive(true);
                _strip.DOFade(1f, 2.5f);

                _pressAnyKey.DOFade(1f, 0f);
                _pressAnyKey.DOFade(0f, 1f).OnComplete(() => _pressAnyKey.gameObject.SetActive(false));
                _pressAnyKeyTransform.DOScale(1.2f, 0f);
                _pressAnyKeyTransform.DOScale(1.4f, 1f);

                GameEventsManager.PlayEvent("Enter");
            }
        }
        else if (_attemptingToConnect && NetworkManager.Connected)
        {
            HideLoading();
            DisplayGameCreation();
            _attemptingToConnect = false;
        }
        else if (_attemptingToJoin && NetworkManager.Hosted)
        {
            HideLoading();
            DisplayPreGameButtons();
            _attemptingToJoin = false;
        }
        else if (_loadingRooms && _roomsLoaded)
        {
            HideLoading();
            DisplayRooms();
            _loadingRooms = false;
        }
    }

    public void OnClickPlayGame()
    {
        _mainMenuButtons.DOFade(0f, .1f).OnComplete(() => _mainMenuButtons.gameObject.SetActive(false));

        if (!NetworkManager.Connected)
        {
            DisplayLoading();
            NetworkManager.Connect();
            _attemptingToConnect = true;
        }
        else
            DisplayGameCreation();
    }

    private void DisplayLoading()
    {
        _loading.gameObject.SetActive(true);
        _loading.DOFade(1f, 1f);

        
        _loadingCircleTransform.DORotate(new Vector3(0, 0, -180), .4f).SetLoops(-1, LoopType.Incremental);
    }

    private void HideLoading()
    {
        _loading.DOFade(0f, .2f).OnComplete(() => _loading.gameObject.SetActive(false));

        _loadingCircleTransform.DOKill();
        _loadingCircleTransform.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
    }

    private void DisplayGameCreation()
    {
        _gameCreationButtons.gameObject.SetActive(true);
        _gameCreationButtons.DOFade(1f, 1f);
    }

    public void OnClickCreateGame()
    {
        _gameCreationButtons.DOComplete();
        _gameCreationButtons.DOFade(0f, .1f).OnComplete(() => _mainMenuButtons.gameObject.SetActive(false));

        NetworkManager.JoinRoom();
        DisplayLoading();

        _attemptingToJoin = true;
    }

    public void OnClickSelectGameToJoin()
    {
        _gameCreationButtons.DOComplete();
        _gameCreationButtons.DOFade(0f, .1f).OnComplete(() => _mainMenuButtons.gameObject.SetActive(false));

        RetrieveRooms();

        DisplayLoading();
    }

    public void OnClickRefreshRooms()
    {
        _roomSelectionBox.DOFade(0f, .2f).OnComplete(() => _roomSelectionBox.gameObject.SetActive(false)); ;
        _roomSelectionButtons.DOFade(0f, .2f).OnComplete(() => _roomSelectionButtons.gameObject.SetActive(false));
        
        RetrieveRooms();
    }

    public void OnClickSelectRoom(MenuRoom room)
    {
        _selectedRoom = room;
        _joinRoomButton.interactable = true;
        _joinRoomMenuButton.UpdateDefaultColor(_defaultColor);
    }

    private void RetrieveRooms()
    {
        NetworkManager.RetrieveRooms();
        _loadingRooms = true;
        _roomsLoaded = false;
    }

    public void LoadRooms(NetworkManager.Room[] rooms)
    {
        _serverRooms = rooms;
        _roomsLoaded = true;
    }

    private void DisplayRooms()
    {
        foreach (NetworkManager.Room room in _serverRooms)
        {
            GameObject newRoom = Instantiate(_roomTemplatePrefab);
            newRoom.transform.SetParent(_roomLayout);

            newRoom.GetComponent<MenuRoom>().Setup(room);
            newRoom.GetComponent<Button>().onClick.AddListener(() => OnClickSelectRoom(newRoom.GetComponent<MenuRoom>()));
        }

        _joinRoomButton.interactable = false;
        _joinRoomMenuButton.UpdateDefaultColor(_defaultDisabledColor);

        _roomSelectionButtons.DOComplete();
        _roomSelectionButtons.gameObject.SetActive(true);
        _roomSelectionButtons.DOFade(1f, 1f);

        _roomSelectionBox.DOComplete();
        _roomSelectionBox.gameObject.SetActive(true);
        _roomSelectionBox.DOFade(1f, 1f);

        _currentCanvas = _roomSelectionButtons;
    }

    public void OnClickJoinSelectedRoom()
    {
        _roomSelectionBox.DOFade(0f, .1f).OnComplete(() => _roomSelectionBox.gameObject.SetActive(false));
        _roomSelectionButtons.DOFade(0f, .1f).OnComplete(() => _roomSelectionButtons.gameObject.SetActive(false));

        NetworkManager.JoinRoom(_selectedRoom.Room);

        DisplayLoading();

        _attemptingToJoin = true;
    }

    private void DisplayPreGameButtons()
    {
        _preGameButtons.gameObject.SetActive(true);
        _preGameButtons.DOFade(1f, 1f);

        _currentCanvas = _preGameButtons;
    }

    public void OnClickCancel()
    {
        _currentCanvas.DOFade(0f, .1f).OnComplete(() => _currentCanvas.gameObject.SetActive(false));

        if(_currentCanvas == _preGameButtons)
            NetworkManager.QuitRoom();
        else
            _roomSelectionBox.DOFade(0f, .1f).OnComplete(() => _currentCanvas.gameObject.SetActive(false));

        _gameCreationButtons.gameObject.SetActive(true);
        _gameCreationButtons.DOFade(1f, 1f);
    }

    public void OnClickQuit()
    {
        _strip.DOFade(0f, .5f).OnComplete(() => _strip.gameObject.SetActive(false));

        _quitBox.gameObject.SetActive(true);
        _quitBox.DOFade(1f, 1f);

        _currentCanvas = _quitBox;
    }

    public void OnClickSettings()
    {
        _strip.DOFade(0f, .5f).OnComplete(() => _strip.gameObject.SetActive(false));

        _settingsBox.gameObject.SetActive(true);
        _settingsBox.DOFade(1f, 1f);

        _currentCanvas = _settingsBox;
    }

    public void OnClickCredits()
    {
        _strip.DOFade(0f, .5f).OnComplete(() => _strip.gameObject.SetActive(false));

        _creditsBox.gameObject.SetActive(true);
        _creditsBox.DOFade(1f, 1f);

        _currentCanvas = _creditsBox;
    }

    public void OnClickQuitConfirmed()
    {
        _quitBox.DOFade(0f, .5f).OnComplete(() => _quitBox.gameObject.SetActive(false));

        _blackScreen.gameObject.SetActive(true);
        _blackScreen.DOFade(1f, 1f).OnComplete(QuitGame);
    }

    public void OnClickBackToMenuRevertButtons()
    {
        _gameCreationButtons.DOFade(0f, .1f).OnComplete(() => _gameCreationButtons.gameObject.SetActive(false));

        _mainMenuButtons.gameObject.SetActive(true);
        _mainMenuButtons.DOFade(1f, 1f);
    }

    public void OnClickBackToMenu()
    {
        _currentCanvas.DOFade(0f, .5f).OnComplete(() => _currentCanvas.gameObject.SetActive(false));

        _strip.gameObject.SetActive(true);
        _strip.DOFade(1f, 1f);
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
