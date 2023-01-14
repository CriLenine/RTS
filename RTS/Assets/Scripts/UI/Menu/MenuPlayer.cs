using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuPlayer : MonoBehaviour
{
    [SerializeField]
    private Image _image;

    [SerializeField]
    private TextMeshProUGUI _playerName, _readyStatus;

    [SerializeField]
    private Color _unreadyColor, _readyColor;

    public void Setup(string playerName, bool status)
    {
        _playerName.text = playerName;
        _readyStatus.text = status ? "Ready" : "Unready";
        _image.color = status ? _readyColor : _unreadyColor;
    }
}
