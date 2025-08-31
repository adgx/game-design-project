using UnityEngine;
namespace ORF.Utils
{
    public static class Math
    {
        public static float NormalizeValueByRage(float minRange, float maxRange, float value)
        {
            return (value - minRange) / (maxRange - minRange);
        }
    }
    public static class GameObjectUtilis
    {
        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform item in gameObject.transform)
            {
                item.gameObject.SetLayerRecursively(layer);
            }
        }
    }

    public enum Layers : int
    {
        Default = 0,
        Ground = 3,
        Player = 6,
        Enemy = 7,
        IgnoreDecals = 8,
        Room = 9,
        Interactable = 10,
        Outline = 11
    }
};