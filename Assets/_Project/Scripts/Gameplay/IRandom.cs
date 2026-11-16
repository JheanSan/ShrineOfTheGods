namespace Shrine.Gameplay
{
    /// <summary>Injectable RNG so battles and AI decisions are reproducible in tests.</summary>
    public interface IRandom
    {
        int Next(int minInclusive, int maxExclusive);
        double NextDouble();
    }

    public sealed class SeededRandom : IRandom
    {
        private readonly System.Random _random;

        public SeededRandom(int seed) => _random = new System.Random(seed);

        public int Next(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);

        public double NextDouble() => _random.NextDouble();
    }
}
