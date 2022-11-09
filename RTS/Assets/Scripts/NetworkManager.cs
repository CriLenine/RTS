using System.Collections.Generic;
using System.Collections;
using PlayerIOClient;
using UnityEngine;

public partial class NetworkManager : MonoBehaviour
{
    public const float TickPeriod = 0.025f;

    private static NetworkManager _instance;

    [SerializeField]
    private bool connect = false;

    private Connection _server;

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
        _messages = new Queue<Message>();
        _ticks = new Dictionary<int, Tick>();

        if (!connect)
            return;

        Connect();
    }

    private void Update()
    {
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

                case "Input":


                    break;

                case "Tick":
                    _ticks.Add(message.GetInt(0), new Tick(message));

                    break;
            }
        }
    }

    public static void Input(TickInput input)
    {
        Message message = Message.Create("Input");

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
                message.Add(input.ID, input.Position);

                break;
        }

        _instance._server.Send(message);
    }

    private IEnumerator Loop()
    {
        yield return new WaitUntil(() => _ticks.ContainsKey(1));

        while (true)
        {
            yield return new WaitUntil(() => connect);

            yield return new WaitForSeconds(TickPeriod);

            yield return new WaitUntil(() => _ticks.ContainsKey(_tick));

            byte[] hash = GameManager.Tick(_ticks[_tick].Inputs);

            _server.Send("Tick", hash);

            _ticks.Remove(_tick);

            Debug.Log($"Tick {_tick}");

            ++_tick;
        }
    }

    private void Connect()
    {
        PlayerIO.Authenticate(
            "rts-q2tnacekgeylj7irzdg",
            "public",
            new Dictionary<string, string> {
                { "userId", SystemInfo.deviceName },
            },
            null,
            delegate (Client client) {
                Debug.Log("Successfully connected to Player.IO");

                client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);

                client.Multiplayer.CreateJoinRoom(
                    "test_room",
                    "Game",
                    true,
                    null,
                    null,
                    delegate (Connection connection) {
                        _server = connection;

                        _server.OnMessage += OnMessage;
                        _server.OnDisconnect += OnDisconnect;

                        _server.Send("Ready");

                        Debug.Log("Joined Room.");
                    },
                    delegate (PlayerIOError error) {
                        Debug.Log("Error joining room: " + error.ToString());
                    }
                );
            },
            delegate (PlayerIOError error) {
                Debug.Log("Error connecting: " + error.ToString());
            }
        );
    }

    private void OnMessage(object sender, Message message)
    {
        _messages.Enqueue(message);
    }

    private void OnDisconnect(object sender, string reason)
    {
        Debug.Log("Disconnected from room.");
    }

    private T[] ExtractArray<T>(Message message, uint startIndex = 0)
    {
        T[] items = new T[message.Count - startIndex];

        for (uint i = startIndex; i < message.Count; ++i)
            items[i - startIndex] = (T)message[i];

        return items;
    }
}
