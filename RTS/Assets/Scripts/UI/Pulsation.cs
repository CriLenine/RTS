using UnityEngine;
using UnityEngine.UI;
using MyBox;

public class Pulsation : MonoBehaviour
{
    enum pulsatingElementType
    {
        SpriteRenderer,
        Image,
        Line
    }

    [Separator("Element Type")]
    [Space]

    [SerializeField]
    private pulsatingElementType elementType;

    [Separator("Pulsating Element")]
    [Space]

    [SerializeField, ConditionalField(nameof(elementType), false, pulsatingElementType.SpriteRenderer)]
    private SpriteRenderer _pulsatingSprite;

    [SerializeField, ConditionalField(nameof(elementType), false, pulsatingElementType.Image)]
    private Image _pulsatingImage;

    [SerializeField, ConditionalField(nameof(elementType), false, pulsatingElementType.Line)]
    private LineRenderer _pulsatingLine;

    private Color _color;

    [Separator("Pulse Specs")]
    [Space]

    [SerializeField]
    private float _pulsePeriod;
    private float _halfPeriod => _pulsePeriod/2;

    [SerializeField, MinMaxRange(0, 1)]
    private RangedFloat _alphaRange;

    private float _timer;

    private void OnEnable()
    {
        _timer = 0f;

        switch (elementType)
        {
            case pulsatingElementType.SpriteRenderer:
                _color = _pulsatingSprite.color;
                break;
            case pulsatingElementType.Image:
                _color = _pulsatingImage.color;
                break;
            case pulsatingElementType.Line:
                _color = _pulsatingLine.startColor;
                break;
        }
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        Color tmp = _color;

        if(_timer < _halfPeriod)
            tmp.a = Mathf.Lerp(_alphaRange.Min, _alphaRange.Max, _timer / _halfPeriod);
        else if (_timer >= _pulsePeriod)
            _timer = 0f;
        else
            tmp.a = Mathf.Lerp(_alphaRange.Max, _alphaRange.Min, _timer / _halfPeriod - 1);

        switch (elementType)
        {
            case pulsatingElementType.SpriteRenderer:
                _pulsatingSprite.color = tmp;
                break;
            case pulsatingElementType.Image:
                _pulsatingImage.color = tmp;
                break;
            case pulsatingElementType.Line:
                _pulsatingLine.startColor = tmp;
                break;
        }
    }
}
