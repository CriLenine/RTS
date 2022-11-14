using System.Collections.Generic;
using PlayerIO.GameLibrary;
using System;

[RoomType("Game")]
public partial class GameRoom : Game<Player>
{
    private const int MaxPlayerCount = 2;

    #region Constructor & Variables

    private List<Player> _players;

    private enum State
    {
        Hosting,
        Loading,
        Playing,
        Pause
    }

    private State _state;

    private Timer _timer;
    private int _tick;

    private Dictionary<int, TickHash> _hashes;
    private Dictionary<int, Message> _ticks;

    public override void GameStarted()
    {
        _players = new List<Player>();

        _hashes = new Dictionary<int, TickHash>();
        _ticks = new Dictionary<int, Message>();

        Reset();
    }

    #endregion

    #region Join & Left

    public override bool AllowUserJoin(Player newcomer)
    {
        return _state == State.Hosting && _players.Count <= MaxPlayerCount;
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

        if (_state != State.Hosting)
            _players.ForEach(p => p.Disconnect());

        _players.Remove(player);
    }

    #endregion

    #region Transition

    private void ResetReady()
    {
        foreach (Player player in _players)
            player.IsReady = false;
    }

    private bool IsReady()
    {
        foreach (Player player in _players)
            if (player.IsReady)
                return false;

        return true;
    }

    private int GetReady()
    {
        int count = 0;

        foreach (Player player in _players)
            if (player.IsReady)
                ++count;

        return count;
    }

    #endregion

    private void Reset()
    {
        _state = State.Hosting;

        _timer?.Stop();
        _tick = 0;

        _hashes.Clear();
        _ticks.Clear();

        foreach (Player player in _players)
            player.Reset();
    }

    private void Start()
    {
        Reset();

        foreach (Player player in _players)
            player.Send("Start");

        _timer = AddTimer(OnTick, Player.TickPeriod);
    }

    public override void GotMessage(Player sender, Message message)
    {
        switch (message.Type)
        {
            case "Ready":
                sender.IsReady = !sender.IsReady;
                
                sender.Send("Ready", sender.IsReady);

                if (_players.Count == MaxPlayerCount && IsReady())
                    Start();

                break;

            case "Count":
                sender.Send("Count", GetReady(), _players.Count);

                if (_players.Count == MaxPlayerCount && IsReady())
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

        if (_hashes[tick].Count >= MaxPlayerCount)
            _hashes.Remove(tick);

        return true;
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

    #region Tools

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

    #endregion
}