using RTS.Feedback;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameEventsManager : MonoBehaviour
{
    private static GameEventsManager _instance;

    [SerializeField]
    private  List<GameEvent> _gameEvents;
    [SerializeField]
    private GameObject _eventEmitter;

    static Dictionary<string, GameEvent> _events;


    private void Awake()
    {
        _instance = this;

        _events = new Dictionary<string, GameEvent>(_gameEvents.Count);

        foreach (var gameEvent in _gameEvents)
        {
            _events.Add(gameEvent.name,gameEvent);
        }
    }

    public static void PlayEvent(string eventName, GameObject gameObject)
    {
        _instance.StartCoroutine(_events[eventName].ExecuteCoroutine(gameObject));
    }

    public static void PlayEvent(string eventName, Vector2 position)
    {
        _instance._eventEmitter.transform.position = position;

        _instance.StartCoroutine(_events[eventName].ExecuteCoroutine(_instance._eventEmitter));
    }

    public static void PlayEvent(string eventName)
    {
        _instance._eventEmitter.transform.position = Vector2.zero;

        _instance.StartCoroutine(_events[eventName].ExecuteCoroutine(_instance._eventEmitter));
    }
}
