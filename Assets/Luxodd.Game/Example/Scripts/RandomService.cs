using System;
using UnityEngine;

namespace Game.Services
{
    public class RandomService
    {
        private System.Random _random;

        public void Init(int seed)
        {
            _random = new System.Random(seed);
        }

        public int Range(int min, int max)
        {
            if (_random == null) throw new InvalidOperationException("RandomService initialized.");
            return _random.Next(min, max);
        }

        public float Value()
        {
            if (_random == null) throw new InvalidOperationException("RandomService not initialized.");
            return (float)_random.NextDouble();
        }

        public float Range(float min, float max)
        {
            return min + Value() * (max - min);
        }
    }
}
