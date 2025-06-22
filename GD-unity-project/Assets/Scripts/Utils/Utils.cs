namespace ORF.Utils
{
    public static class Math
    {
        public static float NormalizeValueByRage(float minRange, float maxRange, float value) 
        {
            return (value - minRange)/(maxRange -minRange);
        }
    }
};