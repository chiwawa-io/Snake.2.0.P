namespace Services.RNG
{
    public interface IRngService
    {
        void Initialize(int seed);
        int NextInt(int min, int max);
        float NextFloat(float min, float max);
    }
}