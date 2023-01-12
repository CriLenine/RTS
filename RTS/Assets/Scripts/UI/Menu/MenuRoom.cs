using TMPro;
using UnityEngine;

public class MenuRoom : MonoBehaviour
{
    public NetworkManager.Room Room { get; private set; }

    [SerializeField]
    TextMeshProUGUI _name, _playersCount, _map;

    public void Setup(NetworkManager.Room room)
    {
        Room = room;

        _name.text = room.Name;
        _playersCount.text = $"{room.Players.Count}/4";
        _map.text = "Dry Arabia";
    }
}
