using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NaiveBruteforcer
{
    internal class Program
    {
        const uint RANGE_MAX = 2147483648;

        static void Main(string[] args)
        {
            var bytes = args.Select(s => byte.Parse(s)).ToImmutableArray();
            var found = false;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var parallelLoopResult = Parallel.For(0, RANGE_MAX, (long seed, ParallelLoopState state) =>
            {
                if (found)
                {
                    state.Stop();
                }

                var rng = new PCG32((uint)seed);
                for (var i = 0; i < bytes.Length; i++)
                {
                    if (rng.NextInt(0, 255) != bytes[i])
                    {
                        return;
                    }

                    if (i == bytes.Length - 1)
                    {
                        Console.WriteLine($"Found the seed '{seed}' in {stopwatch.ElapsedMilliseconds / 1000d:0.000} seconds.");
                        found = true;
                        state.Stop();
                        return;
                    }
                }
            });

            if (!found)
            {
                Console.WriteLine("No seed found for the given dataset!");
            }
        }
    }
}
