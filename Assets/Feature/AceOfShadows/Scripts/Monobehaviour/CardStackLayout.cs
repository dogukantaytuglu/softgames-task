using UnityEngine;

namespace AceOfShadows.Monobehaviour
{
    public static class CardStackLayout
    {
        // Both values come from AceOfShadowsConfig - see the tooltips there for what
        // they mean and why they are tuned the way they are. Passed in rather than
        // read here so this stays a pure function that tests can drive directly.
        public static Vector2 GetOffset(int distanceFromBottom, float perCardOffset, float maxPileRise)
        {
            var y = Mathf.Min(distanceFromBottom * perCardOffset, maxPileRise);
            return new Vector2(0f, y);
        }

        public static Quaternion GetRandomZRotation(float maxDegrees)
        {
            var angle = Random.Range(-maxDegrees, maxDegrees);
            return Quaternion.Euler(0f, 0f, angle);
        }
    }
}
