using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

public class SliderValue : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _value;

    [SerializeField]
    private Slider _slider;

    [SerializeField]
    private VideoPlayer _videoBackground;

    public void UpdateValue()
    {
        int value = (int)(_slider.value * 100);

        _value.text = (value).ToString() + "%";
        AudioListener.volume = _slider.value;

        _videoBackground.SetDirectAudioVolume(0, _slider.value * .5f);
    }
}
