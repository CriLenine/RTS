using PlayerIOClient;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class NetworkManager : MonoBehaviour
{
    public const int MaxPlayerCount = 4;

    private const float BaseTickPeriod = 0.025f;
    private const float MinTickPeriod = 0.005f;

    private static NetworkManager _instance;

    [SerializeField]
    private MenuManager _menuManager;

    #region Public accessors

    // Est connecté à PlayerIO ?
    public static bool IsConnected => _instance._multiplayer != null;

    // A lire uniquement si IsConnected est vrai

    // Est connecté à une room ?
    public static bool IsHosted => _instance._server != null;

    // A lire uniquement si IsHosted est vrai
    
    // Est prêt dans la room ?
    public static bool IsReady => _instance._me.IsReady;

    // Suis-je le host de cette room ?
    public static bool AmIHost { get; private set; } = false;

    // Evénement pour suivre l'évolution d'une room
    public static event OnRoomUpdateHandler OnRoomUpdate
    {
        add => _instance._onRoomUpdate += value;
        remove => _instance._onRoomUpdate -= value;
    }

    // Copie de la dernière version de Room
    public static Room RoomData => _instance._roomData;

    // Une partie est-elle en cours ?
    public static bool IsPlaying => IsHosted && _instance._isPlaying;

    //  A lire uniquement si IsPlaying est vrai //

    // La partie est-elle en play ou en pause ?
    public static bool IsRunning => IsHosted && _instance._isRunning;
    
    // Le temps entre deux lectures de tick
    public static float TickPeriod => _instance._tickPeriod;

    // Le temps par défaut entre deux lectures de tick
    public static float NormalTickPeriod => BaseTickPeriod;

    // Quel performer suis-je ?
    public static int Me => _instance._id;

    public static int PlayerCount => _instance._playerCount;

    public static int AICount => _instance._aiCount;

    // La taille de la room (avec joueurs et ias)
    public static int RoomSize => _instance._roomSize;
    
    // La taille de la room (avec joueurs et ias)
    public static int CurrentTick => _instance._tick;

    #endregion

    #region State

    private Player _me;
    private Room _room;
    private Room _roomData;
    private Connection _server;
    private Multiplayer _multiplayer;

    private bool _isPlaying = false;
    private bool _isRunning = false;

    #endregion

    #region GUI

    private bool _showGUI = true;

    private Room[] _rooms;

    private bool _loading;

    private void OnGUI()
    {
        if (!_showGUI)
            return;

        const int lineHeight = 15;

        int lineCount = 0;

        if (_loading)
            lineCount = 1;
        else if (IsHosted)
        {
            lineCount = 2;

            if (IsPlaying)
                lineCount += 4;
            else
            {
                lineCount += _room.Players != null ? _room.Players.Count : 1;

                if (AmIHost)
                    lineCount += 2;
            }

            lineCount += 2;
        }
        else if (_rooms != null)
            lineCount = 2 + _rooms.Length + 2;
        else if (IsConnected)
            lineCount = 2;

        if (lineCount < 1)
            return;

        GUILayout.BeginArea(new Rect(10, 10, 150, lineHeight * (3 * lineCount + 2) / 2), new GUIStyle(GUI.skin.box));

        if (_loading)
        {
            GUILayout.Label(IsConnected ? "Chargement..." : "Connection...");
        }
        else if (IsHosted)
        {
            GUILayout.Label($"Room {_room.Name}");

            GUILayout.FlexibleSpace();

            if (IsPlaying)
            {
                GUILayout.Label($"Tick N� : {_tick}");
                GUILayout.Label($"Retard : {_lateness}");
                GUILayout.Label($"Tick Period : {_tickPeriod:0.000}");
            }
            else
            {
                foreach (Player player in _room.Players)
                    GUILayout.Label($"{player.Name} - {(player.IsReady ? "O" : "N")}");

                if (AmIHost)
                {
                    GUILayout.FlexibleSpace();

                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Nombre d'IA : ");

                    int maxAICount = MaxPlayerCount - (_room.Players.Count - _room.AiCount);

                    for (int i = 0; i <= maxAICount; ++i)
                    {
                        GUIStyle style = GUI.skin.button;

                        Color color = style.normal.textColor;

                        style.normal.textColor = _room.AiCount == i ? Color.green : Color.red;

                        if (GUILayout.Button($"{i}", style))
                            SendAICount(i);

                        style.normal.textColor = color;
                    }

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.FlexibleSpace();

            if (IsPlaying)
            {
                if (IsRunning)
                {
                    if (GUILayout.Button("Pause"))
                        Pause();
                }
                else
                {
                    if (GUILayout.Button("Play"))
                        Play();
                }
            }
            else
            {
                GUIStyle style = GUI.skin.button;

                Color color = style.normal.textColor;

                style.normal.textColor = IsReady ? Color.green : Color.red;

                if (GUILayout.Button("Pret", style))
                    Ready();

                style.normal.textColor = color;
            }

            if (GUILayout.Button("Quitter"))
                QuitRoom();
        }
        else if (_rooms != null)
        {
            GUILayout.Label("Liste des salons : ");

            GUILayout.FlexibleSpace();

            foreach (Room room in _rooms)
            {
                if (GUILayout.Button($"Rejoindre {room.Name}"))
                {
                    _loading = true;

                    JoinRoom(room.Name, delegate (bool success)
                    {
                        _loading = false;

                        _room = room;

                        _rooms = null;

                        AmIHost = false;
                    });
                }
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Retour"))
                _rooms = null;
        }
        else if (IsConnected)
        {
            GUILayout.Label("Menu de connection");

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Create"))
            {
                _loading = true;

                JoinRoom(_me.Name, delegate (bool success)
                {
                    _loading = false;

                    AmIHost = true;
                });
            }

            if (GUILayout.Button("Join"))
            {
                _loading = true;

                GetRooms(delegate (RoomInfo[] rooms)
                {
                    _loading = false;

                    _rooms = Room.FromRoomInfos(rooms);
                });
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndArea();
    }

    #endregion

    #region Init & Variables

    private float _tickPeriod;

    private int _lateness = 0;

    private bool _wait = false;

    private int _tick;

    private int _roomSize;
    private int _playerCount;
    private int _aiCount;
    private int _id;

    private Dictionary<int, Tick> _ticks;

    private Queue<Message> _messages;

    public delegate void OnRoomUpdateHandler(Room room);

    private event OnRoomUpdateHandler _onRoomUpdate;

    private void Awake()
    {
        _instance = this;

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        _messages = new Queue<Message>();
        _ticks = new Dictionary<int, Tick>();

        _me = new Player(SystemInfo.deviceName, false);
    }

    #endregion

    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.F3))
            _showGUI = !_showGUI;

        _tickPeriod = Mathf.Lerp(_tickPeriod, _lateness > 5 ? MinTickPeriod : BaseTickPeriod, Time.deltaTime);

        #region Messages

        while (_messages.Count > 0 && !_wait)
        {
            Message message = _messages.Dequeue();

            switch (message.Type)
            {
                case "Ready":
                    _me.IsReady = message.GetBoolean(0);

                    break;

                case "Players":
                    _room.Update(message);

                    _roomData = _room.Clone();

                    _onRoomUpdate?.Invoke(_roomData);

                    _loading = false;

                    break;

                case "Start":
                    _isPlaying = true;

                    _tick = 0;

                    _lateness = 0;

                    _tickPeriod = BaseTickPeriod;

                    _ticks.Clear();

                    _id = message.GetInt(1);

                    _playerCount = message.GetInt(0);
                    _aiCount = message.GetInt(2);

                    _roomSize = _playerCount + _aiCount;

                    SetupManager.CompleteReset();

                    AIManager.InitAI(_playerCount, _aiCount);

                    SetupManager.SetupGame();

                    StartCoroutine(Loop());

                    break;

                case "State":
                    _isRunning = message.GetBoolean(0);

                    break;

                case "Tick":
                    Tick tick = new Tick(message);

                    _ticks.Add(message.GetInt(0), tick);

                    ++_lateness;

                    break;

                case "Disconnect":
                    Debug.Log("Disconnect : " + message.GetString(0));

                    QuitRoom();

                    break;
            }
        }

        #endregion
    }

    #region Gameplay

    public static void Input(TickInput input)
    {
        Message message = Message.Create("Input");

        message.Add((int)input.Type);

        switch (input.Type)
        {
            case InputType.QueueSpawn:
                message.Add(input.ID,input.Prefab);

                break;
            case InputType.UnqueueSpawn:
                message.Add(input.ID, input.Prefab);

                break;
            case InputType.UpdateRallyPoint:
                message.Add(input.ID, input.Position.x, input.Position.y);

                break;

            case InputType.Stop:
                Spread(message, input.Targets);

                break;

            case InputType.Kill:
                Spread(message, input.Targets);

                break;

            case InputType.Move:
                message.Add(input.Position.x,input.Position.y);

                Spread(message, input.Targets);

                break;

            case InputType.NewBuild:
                message.Add(input.Prefab, input.Position.x, input.Position.y);

                Spread(message, input.Targets);

                break;


            case InputType.Build:
                message.Add(input.ID);

                Spread(message, input.Targets);

                break;

            case InputType.Destroy:
                message.Add(input.ID);

                break;

            case InputType.CancelConstruction:
                message.Add(input.ID);

                break;

            case InputType.Attack:
                message.Add(input.ID, input.Position.x, input.Position.y);

                Spread(message, input.Targets);

                break;

            case InputType.GuardPosition:
                message.Add(input.Position.x, input.Position.y);

                Spread(message, input.Targets);

                break;

            case InputType.Harvest:
                message.Add(input.Position.x, input.Position.y);

                Spread(message, input.Targets);
                break;
        }

        _instance._server.Send(message);
    }

    private IEnumerator Loop()
    {
        yield return new WaitUntil(() => _ticks.ContainsKey(10));

        while (IsPlaying)
        {
            yield return new WaitForSeconds(_tickPeriod);

            yield return new WaitUntil(() => _ticks.ContainsKey(_tick));

            if (IsRunning)
            {
                int hash = GameManager.Tick(_ticks[_tick].Inputs);
                 
                _server.Send("Tick", hash);

                _ticks.Remove(_tick);

                ++_tick;
                --_lateness;
            }
        }
    }

    #endregion

    #region Play / Pause

    public static void Play()
    {
        if (IsPlaying && !IsRunning)
            _instance._server.Send("Play");
    }

    public static void Pause()
    {
        if (IsRunning)
            _instance._server.Send("Pause");
    }

    #endregion

    #region Connection & Host

    public static void Connect()
    {
        _instance._loading = true;

        PlayerIO.Authenticate(
            "rts-q2tnacekgeylj7irzdg",
            "public",
            new Dictionary<string, string>
            {
                { "userId", _instance._me.Name }
            },
            null,
            delegate (Client client)
            {
                _instance._loading = false;

                //client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);

                _instance._multiplayer = client.Multiplayer;
            },
            delegate (PlayerIOError error)
            {
                _instance._loading = false;
            }
        );
    }

    public static void Ready()
    {
        _instance._server.Send("Ready");
    }

    public static void SendAICount(int count)
    {
        _instance._server.Send("AICount", count);
    }

    public static void GetRooms(Action<RoomInfo[]> callback)
    {
        _instance._multiplayer.ListRooms("Game", null, 0, 0,
            delegate (RoomInfo[] rooms) { callback(rooms); }
        );
    }

    public static void JoinRoom()
    {
        JoinRoom(_instance._me.Name, delegate (bool success)
        {
            _instance._loading = false;

            AmIHost = true;
        });
    }

    public static void JoinRoom(Room room)
    {
        JoinRoom(room.Name, delegate (bool success)
        {
            _instance._loading = false;

            _instance._room = room;

            _instance._rooms = null;

            AmIHost = false;
        });
    }

    private static void JoinRoom(string id, Action<bool> callback)
    {
        _instance._multiplayer.CreateJoinRoom(id, "Game", true, null, null,
            delegate (Connection connection)
            {
                _instance._server = connection;

                _instance._room = new Room(id);

                _instance._server.OnMessage += OnMessage;
                _instance._server.OnDisconnect += OnDisconnect;

                callback(true);
            },
            delegate (PlayerIOError error)
            {
                callback(false);
            }
        );
    }

    public static void QuitRoom()
    {
        if (IsHosted)
        {
            _instance._me.IsReady = false;

            _instance._server.Disconnect();

            _instance.StopCoroutine(_instance.Loop());

            _instance._server = null;

            _instance._isRunning = false;
            _instance._isPlaying = false;
        }
    }

    #endregion

    #region Events

    private static void OnMessage(object sender, Message message)
    {
        _instance._messages.Enqueue(message);
    }

    private static void OnDisconnect(object sender, string reason)
    {
        Debug.Log("Disconnected from room : " + reason);

        QuitRoom();
    }

    private void OnDestroy()
    {
        QuitRoom();
    }

    #endregion

    #region Tools

    private static void Spread<T>(Message message, T[] array)
    {
        message.Add(array.Length);

        for (int i = 0; i < array.Length; ++i)
            message.Add(array[i]);
    }

    private static T[] Extract<T>(Message message, uint startIndex, out uint finalIndex)
    {
        uint count = (uint)message.GetInt(startIndex++);

        finalIndex = startIndex + count;

        T[] items = new T[count];

        for (uint i = startIndex; i < finalIndex; ++i)
            items[i - startIndex] = (T)message[i];

        return items;
    }

    #endregion
}
