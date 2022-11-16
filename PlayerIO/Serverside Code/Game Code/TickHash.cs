public partial class GameRoom
{
    private class TickHash
    {
        public readonly byte[] Hash;
        public int Count { get; private set; }

        public TickHash(byte[] hash)
        {
            Hash = hash;
            Count = 1;
        }

        public bool Compare(byte[] hash)
        {
            ++Count;

            if (hash.Length != Hash.Length)
                return false;

            for (int i = 0; i < hash.Length; ++i)
                if (hash[i] != Hash[i])
                    return false;

            return true;
        }
    }
}