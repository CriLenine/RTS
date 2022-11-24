public partial class GameRoom
{
    private class TickHash
    {
        public readonly int Hash;
        public int Count { get; private set; }

        public TickHash(int hash)
        {
            Hash = hash;
            Count = 1;
        }

        public bool Compare(int hash)
        {
            ++Count;

            return hash == Hash;
        }
    }
}