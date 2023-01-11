using TMPro;
using UnityEngine;
using System.Text;
using MyBox;

public class PostGameScreen : MonoBehaviour
{
    #region Variables

    [Separator("Fields")]
    [Space]

    [SerializeField]
    private TextMeshProUGUI _status;

    [Space]

    [SerializeField]
    private TextMeshProUGUI _duration;

    [Space]

    [SerializeField]
    private TextMeshProUGUI _players;

    [SerializeField]
    private TextMeshProUGUI _unitsKilled, _unitsLost, _buildingsDestroyed, _buildingsLost;

    [Separator("Visuals")]
    [Space]

    [SerializeField]
    private GameObject[] _crowns;

    [Separator("Color Gradients")]
    [Space]

    [SerializeField]
    private TMP_ColorGradient _victoryColorGradient;

    [SerializeField]
    private TMP_ColorGradient _defeatColorGradient;

    private StringBuilder _stringBuilder = new StringBuilder();

    #endregion

    public void Show(bool gameWon)
    {
        gameObject.SetActive(true);

        if (EliminationManager.RemainingPlayersID.Count == 1)
            _crowns[EliminationManager.RemainingPlayersID[0]].SetActive(true);

        _status.text = gameWon ? "VICTORY" : "DEFEAT";
        _status.colorGradientPreset = gameWon ? _victoryColorGradient : _defeatColorGradient;

        _duration.text = HUDManager.GetTimer();

        void SetupStat<T>(T[] values, TextMeshProUGUI field)
        {
            _stringBuilder.AppendLine(values[0].ToString());

            for (int i = 1; i < NetworkManager.RoomSize; ++i)
            {
                _stringBuilder.AppendLine();
                _stringBuilder.AppendLine(values[i].ToString());
            }

            field.text = _stringBuilder.ToString();
            _stringBuilder.Clear();
        }

        SetupStat(NetworkManager.Names, _players);
        SetupStat(StatsManager.UnitsKilledCount, _unitsKilled);
        SetupStat(StatsManager.UnitsLostCount, _unitsLost);
        SetupStat(StatsManager.BuildingsDestroyedCount, _buildingsDestroyed);
        SetupStat(StatsManager.BuildingsLostCount, _buildingsLost);
    }
}
