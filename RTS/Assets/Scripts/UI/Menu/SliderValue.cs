using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SliderValue : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _value;

    [SerializeField]
    private Slider _slider;

    [SerializeField]
    private VideoPlayer _videoBackground;

    public void Setup()
    {
        if (PlayerPrefs.HasKey("volume")) 
        {
            AudioListener.volume = PlayerPrefs.GetFloat("volume");
            _videoBackground.SetDirectAudioVolume(0, PlayerPrefs.GetFloat("volume") * .5f);
            _slider.value = PlayerPrefs.GetFloat("volume");
        }
    }

    public void UpdateValue()
    {
        int value = (int)(_slider.value * 100);

        _value.text = (value).ToString() + "%";
        AudioListener.volume = _slider.value;

        _videoBackground.SetDirectAudioVolume(0, _slider.value * .5f);

        PlayerPrefs.SetFloat("volume", _slider.value);
    }
}
