using UnityEngine;

namespace AceOfShadows.Monobehaviour
{
    // Pure math, no MonoBehaviour - each card's offset is fixed once assigned
    // at placement time (based on its distance from the bottom of whichever
    // stack it just joined), so already-placed cards never need to be
    // repositioned as more cards join on top.
    //
    // Draw order comes from Z position, not SpriteRenderer.sortingOrder - the
    // scene's camera is Perspective with the default transparency sort mode,
    // which already sorts by camera distance. Every card gets a unique Z (not
    // capped like the Y fan) so draw order stays well-defined even past the
    // visual fan cap, where multiple cards share the same Y.
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

        // Not pure (uses UnityEngine.Random) - deliberately not seeded, since
        // this is one-off visual jitter, not something a test needs to
        // reproduce. Called once per card at placement time and never again.
        public static Quaternion GetRandomZRotation(float maxDegrees, float xSeed = 0f, float ySeed = 0f)
        {
            var angle = Random.Range(-maxDegrees, maxDegrees);
            return Quaternion.Euler(xSeed, ySeed, angle);
        }
    }
}
