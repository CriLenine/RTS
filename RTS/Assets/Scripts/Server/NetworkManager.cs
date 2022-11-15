using System.Collections.Generic;
using System.Collections;
using PlayerIOClient;
using UnityEngine;
using System;

public partial class NetworkManager : MonoBehaviour
{
    private const float BaseTickPeriod = 0.025f;
    private const float MinTickPeriod = 0.005f;

    private static NetworkManager _instance;

    private Player _me;
    private Room _room;
    private Connection _server;
    private Multiplayer _multiplayer;

    private bool _isPlaying = false;
    private bool _isRunning = false;

    public static bool Connected => _instance._multiplayer != null;
    public static bool Hosted => _instance._server != null;

    public static bool IsReady => _instance._me.IsReady;
    public static bool IsPlaying => _instance._isPlaying;
    public static bool IsRunning => _instance._isRunning;

    #region GUI

    private bool _showGUI = true;

    private Room[] _rooms;

    private bool _loading;

    #endregion

    public static float TickPeriod { get; private set; }

    private int _lateness = 0;

    private Queue<Message> _messages;

    private bool _wait = false;

    private int _tick;

    private Dictionary<int, Tick> _ticks;

    private void Awake()
    {
        _instance = this;

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        _me = new Player(SystemInfo.deviceName);

        Connect();

        _messages = new Queue<Message>();
        _ticks = new Dictionary<int, Tick>();

        TickPeriod = BaseTickPeriod;
    }

    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.F3))
            _showGUI = !_showGUI;

        TickPeriod = Mathf.Lerp(TickPeriod, _lateness > 5 ? MinTickPeriod : BaseTickPeriod, Time.deltaTime);

        while (_messages.Count > 0 && !_wait)
        {
            Message message = _messages.Dequeue();

            switch (message.Type)
            {
                case "Joined":
                    Debug.Log($"{message.GetString(0)} joined the game");

                    break;

                case "Ready":
                    _me.IsReady = message.GetBoolean(0);

                    break;

                case "Players":
                    _room.Update(message);

                    _loading = false;

                    break;

                case "Start":
                    _isPlaying = true;

                    _tick = 0;

                    StartCoroutine(Loop());

                    Debug.Log($"Start Game");

                    break;

                case "State":
                    _isRunning = message.GetBoolean(0);

                    break;

                case "Tick":
                    Tick tick = new Tick(message);

                    _ticks.Add(message.GetInt(0), tick);

                    ++_lateness;

                    break;
            }
        }
    }

    public static void Input(TickInput input)
    {
        Message message = Message.Create("Input");

        Debug.Log($"Input at {_instance._tick} : {input.Type}");

        message.Add((int)input.Type);

        void Spread<T>(T[] array)
        {
            message.Add(array.Length);

            for (int i = 0; i < array.Length; ++i)
                message.Add(array[i]);
        }

        switch (input.Type)
        {
            case InputType.Spawn:
                message.Add(input.ID, input.Position);

                break;

            case InputType.Move:
                Spread(input.Targets);

                message.Add(input.Position);

                break;

            case InputType.Build:
                message.Add(input.ID, input.Position.x, input.Position.y);

                break;
        }

        _instance._server.Send(message);
    }

    private IEnumerator Loop()
    {
        yield return new WaitUntil(() => _ticks.ContainsKey(1));

        while (IsPlaying)
        {
            yield return new WaitForSeconds(TickPeriod);

            yield return new WaitUntil(() => _ticks.ContainsKey(_tick));

            if (IsRunning)
            {
                if (_ticks[_tick].Inputs.Length > 0)
                    Debug.Log($"Input {_ticks[_tick].Inputs[0].Type} at {_tick}");

                byte[] hash = GameManager.Tick(_ticks[_tick].Inputs);

                _server.Send("Tick", hash);

                _ticks.Remove(_tick);

                ++_tick;
                --_lateness;
            }
        }
    }

    private void OnGUI()
    {
        if (!_showGUI)
            return;

        const int lineHeight = 15;

        int lineCount = 0;

        if (_loading)
            lineCount = 1;
        else if (Hosted)
        {
            lineCount = 2;

            if (IsPlaying)
                lineCount += 4;
            else
                lineCount += _room.Players != null ? _room.Players.Count : 1;

            lineCount += 2;
        }
        else if (_rooms != null)
            lineCount = 2 + _rooms.Length + 2;
        else if (Connected)
            lineCount = 2;

        if (lineCount < 1)
            return;

        GUILayout.BeginArea(new Rect(10, 10, 150, lineHeight * (3 * lineCount + 1) / 2), new GUIStyle(GUI.skin.box));

        if (_loading)
        {
            GUILayout.Label(Connected ? "Chargement..." : "Connection...");
        }
        else if (Hosted)
        {
            GUILayout.Label($"Room {_room.Name}");

            GUILayout.FlexibleSpace();

            if (IsPlaying)
            {
                GUILayout.Label($"Tick N° : {_tick}");
                GUILayout.Label($"Retard : {_lateness}");
                GUILayout.Label($"Tick Period : {TickPeriod:0.000}");
            }
            else
            {
                foreach (Player player in _room.Players)
                    GUILayout.Label($"{player.Name} - {(player.IsReady ? "O" : "N")}");
            }

            GUILayout.FlexibleSpace();

            if(IsPlaying)
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

                if (GUILayout.Button("Prêt", style))
                    _server.Send("Ready");

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
                    });
                }
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Retour"))
                _rooms = null;
        }
        else if (Connected)
        {
            GUILayout.Label("Menu de connection");

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Create"))
            {
                _loading = true;

                JoinRoom(_me.Name, delegate (bool success)
                {
                    _loading = false;
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

    #region Connection & Host

    private static void Connect()
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

                Debug.Log("Successfully connected to Player.IO");

                client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);

                _instance._multiplayer = client.Multiplayer;
            },
            delegate (PlayerIOError error)
            {
                _instance._loading = false;

                Debug.Log("Error connecting: " + error.ToString());
            }
        );
    }

    private static void GetRooms(Action<RoomInfo[]> callback)
    {
        _instance._multiplayer.ListRooms("Game", null, 0, 0,
            delegate (RoomInfo[] rooms) { callback(rooms); }
        );
    }

    private static void JoinRoom(string id, Action<bool> callback)
    {
        _instance._multiplayer.CreateJoinRoom(id, "Game", true, null, null,
            delegate (Connection connection)
            {
                Debug.Log($"Joined room \"{id}\"");

                _instance._server = connection;

                _instance._room = new Room(id);

                _instance._server.OnMessage += OnMessage;
                _instance._server.OnDisconnect += OnDisconnect;

                callback(true);
            },
            delegate (PlayerIOError error)
            {
                Debug.Log($"Error joining room : {error}");

                callback(false);
            }
        );
    }

    public static void QuitRoom()
    {
        if (Hosted)
        {
            _instance._me.IsReady = false;

            _instance._server.Disconnect();

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
        Debug.Log("Disconnected from room.");

        _instance._server = null;
    }

    private void OnDestroy()
    {
        QuitRoom();
    }

    #endregion

    #region Tools

    private T[] ExtractArray<T>(Message message, uint startIndex = 0)
    {
        T[] items = new T[message.Count - startIndex];

        for (uint i = startIndex; i < message.Count; ++i)
            items[i - startIndex] = (T)message[i];

        return items;
    }

    #endregion
}
