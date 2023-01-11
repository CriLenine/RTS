using PlayerIOClient;
using System.Collections.Generic;

public partial class NetworkManager
{
    public class Player
    {
        public string Name;

        public bool IsReady;
        public bool IsAI;

        public Player(string name, bool isAI)
        {
            Name = name;
            IsAI = isAI;
        }
    }

    public class Room
    {
        public string Name;

        public List<Player> Players;

        private int _count;

        public int Count => Players != null ? Players.Count : _count;

        public int AiCount { get; private set; } = 0;

        public Room(string name, int count = 0)
        {
            Name = name;

            _count = count;

            Players = new List<Player>();
        }

        public void Update(Message message)
        {
            Players.Clear();

            AiCount = 0;

            uint i = 1;

            while (i < message.Count)
            {
                Player player = new Player(message.GetString(i++), message.GetBoolean(i++));

                if (player.IsAI)
                    ++AiCount;

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
