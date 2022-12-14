using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Transform _bar;

    [SerializeField]
    private SpriteRenderer _fill;

    [SerializeField]
    private Color _fullHealthColor, _midHealthColor, _lowHealthColor;

    public void SetHealth(float normalizedValue)
    {
        _bar.localScale = new Vector3(normalizedValue, 1);

        _fill.color = normalizedValue > .5f ? Color.Lerp(_midHealthColor, _fullHealthColor, (normalizedValue - .5f) * 2) :
            Color.Lerp(_lowHealthColor, _midHealthColor, normalizedValue * 2);

        gameObject.SetActive(normalizedValue != 1);
    }
}
