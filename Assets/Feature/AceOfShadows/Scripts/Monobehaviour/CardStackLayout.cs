using UnityEngine;

namespace Feature.AceOfShadows.Scripts.Monobehaviour
{
    public static class CardStackLayout
    {
        private const int MaxVisibleDepth = 12;
        private const float PerCardOffset = 0.03f;
        private const float PerCardDepth = 0.002f;

        public static Vector3 GetOffset(int distanceFromBottom)
        {
            var visualDepth = Mathf.Min(distanceFromBottom, MaxVisibleDepth);
            var y = visualDepth * PerCardOffset;
            var z = -distanceFromBottom * PerCardDepth;
            return new Vector3(0f, y, z);
        }

        public static Quaternion GetRandomZRotation(float maxDegrees, float xSeed = 0f, float ySeed = 0f)
        {
            var angle = Random.Range(-maxDegrees, maxDegrees);
            return Quaternion.Euler(xSeed, ySeed, angle);
        }
    }
}
