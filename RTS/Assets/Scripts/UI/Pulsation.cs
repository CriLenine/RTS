using UnityEngine;
using UnityEngine.UI;
using MyBox;
using TMPro;
using DG.Tweening;

public class Pulsation : MonoBehaviour
{
    enum pulsatingElementType
    {
        SpriteRenderer,
        Image,
        Text
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

    [SerializeField, ConditionalField(nameof(elementType), false, pulsatingElementType.Text)]
    private TextMeshProUGUI _pulsatingText;

    [Separator("Pulse Specs")]
    [Space]

    [SerializeField]
    private float _pulsePeriod;
    private float _halfPeriod => _pulsePeriod/2;

    [SerializeField, MinMaxRange(0, 1)]
    private RangedFloat _alphaRange;

    private void OnEnable()
    {
        switch (elementType)
        {
            case pulsatingElementType.SpriteRenderer:
                _pulsatingSprite.DOFade(_alphaRange.Max, 0f);
                _pulsatingSprite.DOFade(_alphaRange.Min, _halfPeriod).SetLoops(-1, LoopType.Yoyo);
                break;
            case pulsatingElementType.Image:
                _pulsatingImage.DOFade(_alphaRange.Max, 0f);
                _pulsatingImage.DOFade(_alphaRange.Min, _halfPeriod).SetLoops(-1, LoopType.Yoyo);
                break;
            case pulsatingElementType.Text:
                _pulsatingText.DOFade(_alphaRange.Max, 0f);
                _pulsatingText.DOFade(_alphaRange.Min, _halfPeriod).SetLoops(-1, LoopType.Yoyo);
                break;
        }
    }

    private void OnDisable()
    {
        switch (elementType)
        {
            case pulsatingElementType.SpriteRenderer:
                _pulsatingSprite.DOKill();
                _pulsatingSprite.DOFade(_alphaRange.Max, 0f);
                break;
            case pulsatingElementType.Image:
                _pulsatingImage.DOKill();
                _pulsatingImage.DOFade(_alphaRange.Max, 0f);
                break;
            case pulsatingElementType.Text:
                _pulsatingText.DOKill();
                _pulsatingText.DOFade(_alphaRange.Max, 0f);
                break;
        }
    }
}
