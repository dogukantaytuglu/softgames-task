using UnityEngine;

namespace AceOfShadows.Monobehaviour
{
    // Pure math, no MonoBehaviour - each card's offset/sorting order is fixed
    // once assigned at placement time (based on its distance from the bottom
    // of whichever stack it just joined), so already-placed cards never need
    // to be repositioned as more cards join on top.
    public static class CardStackLayout
    {
        private const int MaxVisibleDepth = 12;
        private const float PerCardOffset = 0.03f;
        private const int StackOrderSpacing = 1000;

        public static Vector3 GetOffset(int distanceFromBottom)
        {
            var depth = Mathf.Min(distanceFromBottom, MaxVisibleDepth);
            return new Vector3(0f, depth * PerCardOffset, 0f);
        }

        public static int GetSortingOrder(int stackSlot, int distanceFromBottom)
        {
            return stackSlot * StackOrderSpacing + distanceFromBottom;
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
