using System.Collections.Generic;

namespace UnityEngineX
{
    public static class RandomX
    {
        public static int PickRandomIndexFromWeights(float[] weights)
        {
            float totalWeight = weights.Sum();
            return PickRandomIndexFromWeights(weights, totalWeight);
        }

        public static int PickRandomIndexFromWeights(float[] weights, float totalWeight)
        {
            if (totalWeight <= 0)
                return -1;

            float pick = UnityEngine.Random.Range(0, totalWeight);
            for (int i = 0; i < weights.Length; i++)
            {
                pick -= weights[i];
                if (pick <= 0)
                {
                    return i;
                }
            }

            return -1;
        }

        public static int PickRandomIndexFromWeights(List<float> weights)
        {
            float totalWeight = weights.Sum();
            return PickRandomIndexFromWeights(weights, totalWeight);
        }

        public static int PickRandomIndexFromWeights(List<float> weights, float totalWeight)
        {
            if (totalWeight <= 0)
                return -1;

            float pick = UnityEngine.Random.Range(0, totalWeight);
            for (int i = 0; i < weights.Count; i++)
            {
                pick -= weights[i];
                if (pick <= 0)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}