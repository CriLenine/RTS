using System.Collections.Generic;
using PlayerIO.GameLibrary;
using System.Diagnostics;
using System;
using System.Linq;

public class Player : BasePlayer
{
    public const int TickPeriod = 25;

    public int Index;

    public bool IsReady = false;

    private int _pingIndex = 0;
    private readonly int[] _pings = new int[10];

    public int Tick { get; private set; }
    
    public int Ping { get; private set; }

    private readonly Dictionary<int, long> _timestamps = new Dictionary<int, long>();
    private readonly Stopwatch _stopwatch = new Stopwatch();

    public void Reset()
    {
        for (int i = 0; i < _pings.Length; ++i)
            _pings[i] = TickPeriod;

        _pingIndex = 0;

        Ping = TickPeriod;
        Tick = -1;

        _stopwatch.Reset();
    }

    public void Update()
    {
        _pings[_pingIndex] = (int)_stopwatch.ElapsedMilliseconds;

        _pingIndex = (_pingIndex + 1) % _pings.Length;

        Ping = ComputeAveragePing();

        _stopwatch.Restart();
        ++Tick;
    }

    private int ComputeAveragePing()
    {
        int sum = 0;

        for (int i = 0; i < _pings.Length; ++i)
            sum += _pings[i];

        return sum / _pings.Length;
    }
}

[RoomType("Game")]
public class GameRoom : Game<Player>
{
    const int maxPlayerCount = 2;

    private List<Player> _players;

    private bool _isRunning = false;

    private Timer _timer;
    private int _tick;

    private Dictionary<int, int[]> _hashes;
    private Dictionary<int, Message> _ticks;

    public override void GameStarted()
    {
        _players = new List<Player>();

        _hashes = new Dictionary<int, int[]>();
        _ticks = new Dictionary<int, Message>();
    }

    public override bool AllowUserJoin(Player newcomer)
    {
        return !_isRunning && _players.Count <= maxPlayerCount;
    }

    public override void UserJoined(Player newcomer)
    {
        _players.Add(newcomer);

        newcomer.Index = _players.Count;

        foreach (Player player in _players)
            player.Send("Joined", player.ConnectUserId);
    }

    public override void UserLeft(Player player)
    {
        if (_isRunning)
            _players.ForEach(p => p.Disconnect());
    }

    public override void GotMessage(Player sender, Message message)
    {
        switch (message.Type)
        {
            case "Ready":
                sender.IsReady = !sender.IsReady;

                sender.Send("Ready", sender.IsReady);

                if (_players.Count == maxPlayerCount && IsReady())
                    Start();

                break;

            case "Input":
                Prepare(sender.Index, message);

                break;

            case "Tick":
                sender.Update();

                //Console.WriteLine($"Tick {sender.Tick} : {sender.ConnectUserId} ({sender.Ping}ms)");

                break;
        }
    }

    private Message Prepare(int tick)
    {
        if (!_ticks.ContainsKey(tick))
            _ticks[tick] = Message.Create("Tick", tick);

        return _ticks[tick];
    }

    private void Prepare(int performer, Message input)
    {
        int tick = _tick + MaxPing() / Player.TickPeriod + 2;

        Console.WriteLine($"Input from {performer} : {input.GetInt(0)} (Received at {_tick} but delayed to {tick})");

        Message message = Prepare(tick);

        message.Add(performer);

        for (uint i = 0; i < input.Count; i++)
            message.Add(input[i]);
    }

    private void Start()
    {
        _timer?.Stop();

        _tick = 0;

        _ticks.Clear();

        foreach (Player player in _players)
        {
            player.Reset();

            player.Send("Start");
        }

        _timer = AddTimer(OnTick, Player.TickPeriod);
    }

    private void OnTick()
    {
        //Console.WriteLine($"Start tick {_tick}");

        Message message = Prepare(_tick);

        foreach (Player player in _players)
            player.Send(message);

        _ticks.Remove(_tick);

        ++_tick;
    }

    private int MaxPing()
    {
        int max = -1;

        foreach (Player player in _players)
            if (player.Ping > max)
                max = player.Ping;

        return max;
    }

    private void ResetPlayers()
    {
        foreach (Player player in _players)
            player.IsReady = false;
    }

    private bool IsReady()
    {
        foreach (Player player in _players)
            if (!player.IsReady)
                return false;

        return true;
    }

    private Message CreateMessage<T>(string type, List<T> list, params object[] parameters)
    {
        return CreateMessage(type, list.ToArray(), parameters);
    }

    private Message CreateMessage<T>(string type, T[] list, params object[] parameters)
    {
        Message message = Message.Create(type);

        foreach (object parameter in parameters)
            message.Add(parameter);

        foreach (T item in list)
            message.Add(item);

        return message;
    }
}