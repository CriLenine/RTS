using PlayerIOClient;
using System.Collections.Generic;

public partial class NetworkManager
{
    public class Player
    {
        public string Name;

        public bool IsReady;

        public Player(string name)
        {
            Name = name;
        }
    }

    public class Room
    {
        public string Name;

        public List<Player> Players;

        private int _count;

        public int Count
        {
            get => Players != null ? Players.Count : _count;
        }

        public Room(string name, int count = 0)
        {
            Name = name;

            _count = count;

            Players = new List<Player>();
        }

        public void Update(Message message)
        {
            Players.Clear();

            uint i = 1;

            while (i < message.Count)
            {
                Player player = new Player(message.GetString(i++));

                player.IsReady = message.GetBoolean(i++);

                Players.Add(player);
            }
        }

        public static Room[] FromRoomInfos(RoomInfo[] infos)
        {
            Room[] rooms = new Room[infos.Length];

            for (int i = 0; i < infos.Length; ++i)
                rooms[i] = new Room(infos[i].Id, infos[i].OnlineUsers);

            return rooms;
        }
    }
}
