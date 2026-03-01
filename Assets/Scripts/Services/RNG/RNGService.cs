using System;

namespace Services.RNG
{
    public class RngService : IRngService
    {
        private Random _random;

        public void Initialize(int seed)
        {
            _random = new Random(seed);
        }

        public int NextInt(int min, int max)
        {
            if (_random == null) throw new Exception("RNG Service not initialized with seed!");
            
            return _random.Next(min, max);
        }

        public float NextFloat(float min, float max)
        {
            if (_random == null) throw new Exception("RNG Service not initialized with seed!");

            double range = (double)max - min;
            double sample = _random.NextDouble();
            double scaled = (sample * range) + min;
            return (float)scaled;
        }
    }
}