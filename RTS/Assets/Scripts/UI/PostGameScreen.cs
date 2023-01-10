using TMPro;
using UnityEngine;
using System.Text;
using MyBox;

public class PostGameScreen : MonoBehaviour
{
    [Separator("Fields")]
    [Space]

    [SerializeField]
    private TextMeshProUGUI _status;

    [Space]

    [SerializeField]
    private TextMeshProUGUI _players, _unitsKilled, _unitsLost, _buildingsCreated, _buildingsLost;

    [Separator("Color Gradients")]
    [Space]

    [SerializeField]
    private TMP_ColorGradient _victoryColorGradient;

    [SerializeField]
    private TMP_ColorGradient _defeatColorGradient;

    private StringBuilder _stringBuilder = new StringBuilder();

    public void Show(bool gameWon)
    {
        this.gameObject.SetActive(true);

        _status.text = gameWon ? "VICTORY" : "DEFEAT";
        _status.colorGradientPreset = gameWon ? _victoryColorGradient : _defeatColorGradient;
    }
}
