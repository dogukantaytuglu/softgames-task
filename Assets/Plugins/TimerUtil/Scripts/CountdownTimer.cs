using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TimerUtil
{
    public class CountdownTimer : Timer
    {
        private readonly bool _useRandomDuration;
        private readonly Vector2 _randomDuration;

        private int _loopCount;
        private int _remainingLoopCount;
        private float _initialDuration;
        private float _duration;

        public bool DestroyOnComplete;
        public Action OnCountdownComplete;
        public Action OnLoopComplete;

        public float CountdownPercent => _seconds / _initialDuration;
        public float InitialDuration => _initialDuration;

        public CountdownTimer(float duration, int loopCount = 1) : this(null, duration, loopCount) {}

        public CountdownTimer(float duration, Action onComplete, int loopCount = 1) : this(null, duration, loopCount)
        {
            OnCountdownComplete = onComplete;
        }

        public CountdownTimer(string id, float duration, int loopCount = 1) : base(id)
        {
            _useRandomDuration = false;
            _seconds = _duration = duration;

            _loopCount = loopCount;
        }

        public CountdownTimer(Vector2 randomDuration, int loopCount = 1) : this(null, randomDuration, loopCount) {}
        public CountdownTimer(string id, Vector2 randomDuration, int loopCount = 1) : base(id)
        {
            _useRandomDuration = true;
            _randomDuration = randomDuration;

            _loopCount = loopCount;
        }

        protected override void PrepareStart()
        {
            ResetDuration();
            _remainingLoopCount = _loopCount;
        }

        private void ResetDuration()
        {
            _initialDuration = GetDuration();
            _seconds = _initialDuration;
        }

        protected override void Tick()
        {
            if (_seconds > 0f)
            {
                _seconds -= Time.deltaTime;
                OnTicked?.Invoke();
            }
            else
            {
                OnTimeout();
            }
        }

        void OnTimeout()
        {
            if (_remainingLoopCount != -1)
                _remainingLoopCount--;

            if (_remainingLoopCount == 0)
            {
                OnCountdownComplete?.Invoke();
                Stop();
                if (DestroyOnComplete)
                {
                    Destroy();
                }
            }
            else
            {
                OnLoopComplete?.Invoke();
                ResetDuration();
            }
        }
        
        public void SetLoopCount(int loopCount)
        {
            _loopCount = loopCount;
            _remainingLoopCount = loopCount;
        }

        public void SetDuration(float duration)
        {
            _seconds = _duration = duration;
        }

        private float GetDuration()
        {
            if (_useRandomDuration)
                return Random.Range(_randomDuration.x, _randomDuration.y);

            return _duration;
        }
        
        private void Destroy()
        {
            TimerService.UnregisterTimer(this);
        }

    }
}
