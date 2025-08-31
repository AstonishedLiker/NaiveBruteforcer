namespace NaiveBruteforcer
{
    internal class PCG32
    {
        private const ulong PCG32_INC = 105;
        private ulong state;

        public PCG32(ulong seed)
        {
            state = 0UL;
            NextUInt();
            state += seed;
            NextUInt();
        }

        public uint NextUInt()
        {
            var oldstate = state;
            state = (oldstate * 6364136223846793005UL) + (PCG32_INC | 1);

            var xorshifted = (uint)(((oldstate >> 18) ^ oldstate) >> 27);
            var rot = (uint)(oldstate >> 59);

            return (xorshifted >> (int)rot) | (xorshifted << ((-(int)rot) & 31));
        }

        public int NextInt(int min, int max)
        {
            var delta = (uint)(max - min);
            var x = (ulong)(delta + 1) * NextUInt();
            return (int)((ulong)min + (x >> 32));
        }
    }
}
