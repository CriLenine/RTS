using System.Collections.Generic;
using PlayerIO.GameLibrary;
using System.Diagnostics;
using System;

public class Player : BasePlayer
{
    public const int TickPeriod = 25;

    public int Index;

    public bool IsReady = false;

    private int _pingIndex = 0;
    private readonly int[] _pings;

    public int Tick { get; private set; }
    
    public int Ping { get; private set; }

    private readonly Dictionary<int, int> _timestamps;
    private readonly Stopwatch _stopwatch;

    public Player()
    {
        _pings = new int[10];

        _timestamps = new Dictionary<int, int>();
        _stopwatch = new Stopwatch();
    }

    public void Reset()
    {
        for (int i = 0; i < _pings.Length; ++i)
            _pings[i] = TickPeriod;

        _pingIndex = 0;

        Ping = TickPeriod;
        Tick = 0;

        _stopwatch.Restart();
    }

    public void StartTick(int tick)
    {
        _timestamps.Add(tick, (int)_stopwatch.ElapsedMilliseconds);
    }

    public void EndTick()
    {
        _pings[_pingIndex] = (int)_stopwatch.ElapsedMilliseconds - _timestamps[Tick];

        _timestamps.Remove(Tick);

        Ping = ComputeAveragePing();

        _pingIndex = (_pingIndex + 1) % _pings.Length;

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
    private class TickHash
    {
        public readonly byte[] Hash;
        public int Count { get; private set; }

        public TickHash(byte[] hash)
        {
            Hash = hash;
            Count = 1;
        }

        public bool Compare(byte[] hash)
        {
            ++Count;

            if (hash.Length != Hash.Length)
                return false;

            for (int i = 0; i < hash.Length; ++i)
                if (hash[i] != Hash[i])
                    return false;

            return true;
        }
    }

    private const int _maxPlayerCount = 1;

    private List<Player> _players;

    private bool _isRunning = false;

    private Timer _timer;
    private int _tick;

    private Dictionary<int, TickHash> _hashes;
    private Dictionary<int, Message> _ticks;

    public override void GameStarted()
    {
        _players = new List<Player>();

        _hashes = new Dictionary<int, TickHash>();
        _ticks = new Dictionary<int, Message>();
    }

    public override bool AllowUserJoin(Player newcomer)
    {
        return !_isRunning && _players.Count <= _maxPlayerCount;
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
        _timer.Stop();
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

                if (_players.Count == _maxPlayerCount && IsReady())
                    Start();

                break;

            case "Input":
                Prepare(sender.Index, message);

                break;

            case "Tick":
                sender.EndTick();

                if (!Compare(sender.Tick, message.GetByteArray(0)))
                    Console.WriteLine($"Wrong hash {sender.Index}");

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

        Message message = Prepare(tick);

        message.Add(performer);

        for (uint i = 0; i < input.Count; i++)
            message.Add(input[i]);
    }

    private bool Compare(int tick, byte[] hash)
    {
        if (!_hashes.ContainsKey(tick))
            _hashes.Add(tick, new TickHash(hash));

        if (!_hashes[tick].Compare(hash))
            return false;

        if (_hashes[tick].Count >= _maxPlayerCount)
            _hashes.Remove(tick);

        return true;
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
        Message message = Prepare(_tick);

        foreach (Player player in _players)
        {
            player.Send(message);

            player.StartTick(_tick);
        }

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