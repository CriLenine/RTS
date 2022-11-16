using System.Collections.Generic;
using PlayerIO.GameLibrary;
using System.Diagnostics;

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