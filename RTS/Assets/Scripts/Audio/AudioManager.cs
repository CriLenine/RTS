using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    [SerializeField]
    private GameObject _audioEmitter;

    [SerializeField]
    private AudioSource _alertSource;

    [SerializeField]
    private AudioSource _musicSource;

    [SerializeField]
    private AudioSource _spawnBlueprintSource;

    [SerializeField]
    private AudioSource _spawnBuildingSource;

    private List<AudioSource> _audioSources = new List<AudioSource>();

    private bool _alert = false;
    private float _timeSinceAlert = 0f;

    private void Awake()
    {
        _instance ??= this;
    }

    private void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            AudioSource newSource = _instance._audioEmitter.AddComponent<AudioSource>();
            _instance._audioSources.Add(newSource);
        }
    }

    private void Update()
    {
        if ((_timeSinceAlert += Time.deltaTime) > 5f)
        {
            EndAlert();
        }        
    }

    public static void StartGame()
    {
        _instance._musicSource.Play();
        _instance._alertSource.Play();
    }

    public static void PlayNewSound(AudioClip clip)
    {
        for (int i = 0; i < _instance._audioSources.Count; i++)
        {
            AudioSource source = _instance._audioSources[i];
            if (!source.isPlaying)
            {
                source.clip = clip;
                source.Play();
                return;
            }
        }

        AudioSource newSource = _instance._audioEmitter.AddComponent<AudioSource>();
        _instance._audioSources.Add(newSource);
        newSource.clip = clip;
        newSource.Play();
    }

    public static void PlayBuildingSound()
    {
        _instance._spawnBuildingSource.Stop();
        _instance._spawnBuildingSource.Play();
    }

    public static void PlayBlueprintSound()
    {
        _instance._spawnBlueprintSource.Stop();
        _instance._spawnBlueprintSource.Play();
    }

    public static void TriggerAlert()
    {
        _instance._timeSinceAlert = 0f;
        if (_instance._alert)
            return;
        _instance._alert = true;

        _instance.StartCoroutine(StartFade(_instance._musicSource, 2f, 0f));
        _instance.StartCoroutine(StartFade(_instance._alertSource, 2f, 1f));
    }

    private static void EndAlert()
    {
        if (!_instance._alert)
            return;

        _instance._alert = false;

        _instance.StartCoroutine(StartFade(_instance._musicSource, 1f, 1f));
        _instance.StartCoroutine(StartFade(_instance._alertSource, 1f, 0f));
    }

    private static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = audioSource.volume;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        yield break;
    }
}
