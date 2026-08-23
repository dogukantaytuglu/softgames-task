using UnityEngine;

namespace AceOfShadows.Monobehaviour
{
    public static class CardStackLayout
    {
        private const int MaxVisibleDepth = 12;

        // Eyeballed pixel equivalent of the old 0.03 world-unit fan offset,
        // scaled to the new ~260px card height. Retune by eye in Play Mode.
        private const float PerCardOffset = 3f;

        public static Vector2 GetOffset(int distanceFromBottom)
        {
            var visualDepth = Mathf.Min(distanceFromBottom, MaxVisibleDepth);
            var y = visualDepth * PerCardOffset;
            return new Vector2(0f, y);
        }

        public static Quaternion GetRandomZRotation(float maxDegrees)
        {
            var angle = Random.Range(-maxDegrees, maxDegrees);
            return Quaternion.Euler(0f, 0f, angle);
        }
    }
}
