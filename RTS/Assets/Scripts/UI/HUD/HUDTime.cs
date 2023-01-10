using System.Text;
using TMPro;
using UnityEngine;

public class HUDTime : HUD
{
    [SerializeField]
    private TextMeshProUGUI _time;

    private StringBuilder _text;

    private int _minutes, _seconds;

    private float _timer = 0f;
    private bool _timerStarted = false;

    public void StartTimer()
    {
        _timerStarted = true;
        _text = new StringBuilder();
    }

    private void Update()
    {
        if (!_timerStarted)
            return;

        _timer += Time.deltaTime;

        if (_timer < 1)
            return;

        _timer = 0f;

        _text.Clear();

        if (++_seconds >= 60)
        {
            ++_minutes;
            _seconds = 0;
        }

        _text.AppendFormat(_minutes >= 10 ? "{0}" : "0{0}", _minutes);
        _text.Append(":");
        _text.AppendFormat(_seconds >= 10 ? "{0}" : "0{0}", _seconds);

        _time.text = _text.ToString();
    }
}
