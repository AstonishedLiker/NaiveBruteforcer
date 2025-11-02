// This code is NOT optimized (notably PCG32 algo)!
// This is a demo.
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;

namespace NaiveBruteforcer
{
    internal class Program
    {
        const long RANGE_MAX = 2_147_483_648L;

        static void Main(string[] args)
        {
            var bytes = args.Select(s => byte.Parse(s)).ToImmutableArray();
            var stopwatch = Stopwatch.StartNew();

            var winnerSeed = -1L;
            using var ctSource = new CancellationTokenSource();

            var parallelOptions = new ParallelOptions
            {
                CancellationToken = ctSource.Token
            };

            var bytesLength = bytes.Length;
            var partitioner = Partitioner.Create(0L, RANGE_MAX, 1_000_000L);

            bool Found() => Interlocked.Read(ref winnerSeed) != -1L;
            try
            {
                Parallel.ForEach(partitioner, parallelOptions, (range, state) =>
                {
                    var token = parallelOptions.CancellationToken;
                    if (Found() || token.IsCancellationRequested)
                        return;

                    const int checkStride = 16384; // power of 2, because we use bitwise AND shortcut
                    for (var seed = range.Item1; seed < range.Item2; seed++)
                    {
                        if ((seed & (checkStride - 1)) == 0 && (Found() || token.IsCancellationRequested))
                            return;

                        var rng = new PCG32((uint)seed);
                        var i = 0;
                        for (; i < bytesLength; i++)
                        {
                            if (rng.NextInt(0, 255) != bytes[i])
                                break;
                        }

                        if (i == bytesLength)
                        {
                            if (Interlocked.CompareExchange(ref winnerSeed, seed, -1) == -1)
                            {
                                Console.WriteLine($"Found the seed '{seed}' in {stopwatch.ElapsedMilliseconds / 1000D:0.000} seconds.");
                                ctSource.Cancel();
                                state.Stop();
                            }
                            return;
                        }
                    }
                });
            }
            catch (OperationCanceledException) { } // expected

            stopwatch.Stop();
            if (Interlocked.Read(ref winnerSeed) == -1)
            {
                Console.WriteLine("No seed found for the given dataset!");
            }
        }
    }
}