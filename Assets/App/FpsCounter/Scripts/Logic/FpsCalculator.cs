using System;

namespace FpsCounter.Logic
{
    public class FpsCalculator
    {
        private readonly float _updateInterval;
        private float _accumulatedTime;
        private int _frameCount;

        public float CurrentFps { get; private set; }

        public FpsCalculator(float updateInterval = 0.5f)
        {
            if (updateInterval <= 0f)
                throw new ArgumentOutOfRangeException(nameof(updateInterval));

            _updateInterval = updateInterval;
        }

        // Averaged over updateInterval rather than 1f / deltaTime per frame,
        // which reads as noisy jitter rather than a stable number.
        public void Sample(float deltaTime)
        {
            _accumulatedTime += deltaTime;
            _frameCount++;

            if (_accumulatedTime < _updateInterval)
                return;

            CurrentFps = _frameCount / _accumulatedTime;
            _accumulatedTime = 0f;
            _frameCount = 0;
        }
    }
}
