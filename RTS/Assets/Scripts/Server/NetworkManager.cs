using System.Collections.Generic;
using System.Collections;
using PlayerIOClient;
using UnityEngine;
using System;

public partial class NetworkManager : MonoBehaviour
{
    private const float BaseTickPeriod = 0.025f;
    private const float MinTickPeriod = 0.005f;

    public static float TickPeriod { get; private set; }

    private int _lateness = 0;

    private static NetworkManager _instance;

    private Connection _server;

    private Queue<Message> _messages;

    private bool _wait = false;

    private int _tick;

    private Dictionary<int, Tick> _ticks;

    #region Connection

    private bool _showGUI = true;

    private Multiplayer _multiplayer;

    private RoomInfo[] _rooms;

    private bool _loading;
    private bool _connected;
    private int _playerCount;

    #endregion

    private void Awake()
    {
        _instance = this;

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
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

                case "Start":
                    Debug.Log($"Start Game");

                    _tick = 0;

                    StartCoroutine(Loop());

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

        while (true)
        {
            yield return new WaitForSeconds(TickPeriod);

            yield return new WaitUntil(() => _ticks.ContainsKey(_tick));

            if (_ticks[_tick].Inputs.Length > 0)
                Debug.Log($"Input {_ticks[_tick].Inputs[0].Type} at {_tick}");

            byte[] hash = GameManager.Tick(_ticks[_tick].Inputs);

            _server.Send("Tick", hash);

            _ticks.Remove(_tick);

            ++_tick;
            --_lateness;
        }
    }

    private void OnGUI()
    {
        if (!_showGUI)
            return;

        const int lineHeight = 15;

        int lineCount;

        if (_loading)
            lineCount = 1; // Loading...
        else if (_server != null)
            lineCount = 1; // Connected
        else if (_rooms != null)
            lineCount = _rooms.Length + 1; // List rooms
        else if (_multiplayer != null)
            lineCount = 2; // Host/ Join
        else
            lineCount = 1; // Connecting...

        /*
        GUILayout.Label($"Retard : {_lateness}");
        GUILayout.Label($"Tick Period : {TickPeriod:0.000}");
        */

        GUILayout.BeginArea(new Rect(10, 10, 150, lineHeight * (3 * lineCount + 1) / 2), new GUIStyle(GUI.skin.box));

        if (_loading)
        {
            GUILayout.Label("Chargement...");
        }
        else if (_server != null)
        {
            GUILayout.Label("Connected");
        }
        else if (_rooms != null)
        {
            GUILayout.Label("Liste de rooms : ");

            foreach (RoomInfo room in _rooms)
            {
                if (GUILayout.Button($"Rejoindre {room.Id}"))
                {
                    _loading = true;

                    JoinRoom(room.Id, delegate (bool success)
                    {
                        _loading = false;

                        _rooms = null;
                    });
                }
            }   
        }
        else if (_multiplayer != null) // Host/ Join
        {
            GUILayout.Label("Room menu");

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Create"))
            {
                _loading = true;

                JoinRoom(SystemInfo.deviceName, delegate (bool success)
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

                    _rooms = rooms;
                });
            }

            GUILayout.EndHorizontal();
        }
        else // Connecting...
        {
            GUILayout.Label("Connecting...");
        }

        GUILayout.EndArea();
    }

    private static void Connect()
    {
        PlayerIO.Authenticate(
            "rts-q2tnacekgeylj7irzdg",
            "public",
            new Dictionary<string, string>
            {
                { "userId", SystemInfo.deviceName }
            },
            null,
            delegate (Client client)
            {
                Debug.Log("Successfully connected to Player.IO");

                client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);

                _instance._multiplayer = client.Multiplayer;
            },
            delegate (PlayerIOError error)
            {
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
        _server?.Disconnect();
    }

    private T[] ExtractArray<T>(Message message, uint startIndex = 0)
    {
        T[] items = new T[message.Count - startIndex];

        for (uint i = startIndex; i < message.Count; ++i)
            items[i - startIndex] = (T)message[i];

        return items;
    }
}
