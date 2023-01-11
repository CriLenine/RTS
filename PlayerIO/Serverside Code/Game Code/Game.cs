using System.Collections.Generic;
using PlayerIO.GameLibrary;
using System;

[RoomType("Game")]
public partial class GameRoom : Game<Player>
{
    private const int MaxPlayerCount = 4;

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

    private bool IsHosting => _state == State.Hosting;
    private bool IsPaused => _state == State.Pause;
    private bool IsRunning => _state == State.Playing;

    private bool IsPlaying => IsRunning || IsPaused;

    private Timer _timer;
    private int _tick;

    private int __iaCount = 0;
    private int _iaCount
    {
        get => Math.Min(__iaCount, MaxPlayerCount - _players.Count);

        set => __iaCount = Math.Max(0, Math.Min(value, 3));
    }

    private Dictionary<int, TickHash> _hashes;
    private Dictionary<int, Message> _ticks;
    
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

    public override void GameStarted()
    {
        _players = new List<Player>();

        _hashes = new Dictionary<int, TickHash>();
        _ticks = new Dictionary<int, Message>();

        Reset();
    }

    public override void GameClosed()
    {
        DisconnectAll("Game closed");
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

        UpdateIndexes();

        BroadcastPlayers();
    }

    public override void UserLeft(Player player)
    {
        if (_state == State.Hosting)
        {
            _players.Remove(player);

            UpdateIndexes();
        }
        else
            DisconnectAll("User left");
    }

    private void DisconnectAll(string reason)
    {
        foreach (Player player in _players)
        {
            player.Send("Disconnect", reason);

            player.Disconnect();
        }

        _players.Clear();
    }

    private void UpdateIndexes()
    {
        for (int i = 0; i < _players.Count; ++i)
            _players[i].Index = i;
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
            if (!player.IsReady)
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

    #region Broadcast

    private void BroadcastPlayers()
    {
        Message message = Message.Create("Players");

        message.Add(_players.Count + _iaCount);

        foreach (Player player in _players)
            message.Add(player.ConnectUserId, false, player.IsReady);

        for (int i = 0; i < _iaCount; ++i)
            message.Add($"Bot {i + 1}", true, true);

        Broadcast(message);
    }

    private void BroadcastState()
    {
        Broadcast("State", IsRunning);
    }

    #endregion

    #region Play / Pause

    private void Start()
    {
        if (!IsPlaying)
        {
            Reset();

            foreach (Player player in _players)
                player.Send("Start", _players.Count, player.Index, _iaCount);

            _iaCount = 0;
        }

        _state = State.Playing;

        _timer = AddTimer(OnTick, Player.TickPeriod);

        BroadcastState();
    }

    private void Pause()
    {
        _state = State.Pause;

        _timer.Stop();

        BroadcastState();
    }

    #endregion

    #region Messages

    public override void GotMessage(Player sender, Message message)
    {
        switch (message.Type)
        {
            case "Ready":
                sender.IsReady = !sender.IsReady;

                sender.Send("Ready", sender.IsReady);

                BroadcastPlayers();

                if (_state == State.Hosting && IsReady())
                    Start();

                break;

            case "AICount":
                if (IsHosting && sender == _players[0])
                {
                    _iaCount = message.GetInt(0);

                    BroadcastPlayers();
                }

                break;

            case "Input":
                if (IsRunning)
                    Prepare(sender.Index, message);

                break;

            case "Play":
                if (IsPaused)
                    Start();

                break;

            case "Pause":
                if (IsRunning)
                    Pause();

                break;

            case "Tick":
                if (IsRunning)
                {
                    sender.EndTick();

                    if (!Compare(sender.Tick, message.GetInt(0)))
                        DisconnectAll("Wrong hash");
                }

                break;
        }
    }

    #endregion

    #region Gameplay

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

    private bool Compare(int tick, int hash)
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

    #endregion

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