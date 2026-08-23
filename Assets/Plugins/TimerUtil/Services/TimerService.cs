using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace TimerUtil
{
    public static class TimerService
    {
        private static readonly List<Timer> Timers = new();
        private static TimerTicker _timerTicker;

        public static ReadOnlyCollection<Timer> AllTimers => Timers.AsReadOnly();

        public static CountdownTimer CreateCountdownTimer(float duration, int loopCount = 1)
            => CreateCountdownTimer(null, duration, loopCount);

        public static CountdownTimer CreateCountdownTimer(string id, float duration, int loopCount = 1)
        {
            return new CountdownTimer(id, duration, loopCount);
        }

        public static CountdownTimer CreateCountdownTimer(Vector2 randomDuration, int loopCount = 1)
            => CreateCountdownTimer(null, randomDuration, loopCount);

        public static CountdownTimer CreateCountdownTimer(string id, Vector2 randomDuration, int loopCount = 1)
        {
            return new CountdownTimer(id, randomDuration, loopCount);
        }

        public static StopwatchTimer CreateStopwatch(string id = null)
        {
            return new StopwatchTimer(id);
        }

        public static void RegisterTimer(Timer timer)
        {
            if (Timers.Contains(timer)) return;

            TryUnregisterTimerWithSameId(timer);

            ValidateTimerTicker();

            Timers.Add(timer);
        }

        private static void ValidateTimerTicker()
        {
            if (_timerTicker) return;
            _timerTicker = CreateTimerTicker();
        }

        private static void TryUnregisterTimerWithSameId(Timer timer)
        {
            var timerId = timer.Id;
            var isValidId = !string.IsNullOrEmpty(timerId);

            if (isValidId && Timers.Exists(t => t.Id == timerId))
            {
                UnregisterTimer(timer);
            }
        }

        public static Timer FindTimerById(string id)
        {
            return Timers.Find(t => t.Id == id);
        }

        public static void UnregisterTimer(string id)
        {
            var timer = FindTimerById(id);
            if (timer == null) return;

            UnregisterTimer(timer);
        }

        public static void UnregisterTimer(Timer timer)
        {
            Timers.Remove(timer);
        }

        private static TimerTicker CreateTimerTicker()
        {
            var timersGameObject = new GameObject(nameof(TimerTicker));
            Object.DontDestroyOnLoad(timersGameObject);
            var timerTicker = timersGameObject.AddComponent<TimerTicker>();
            return timerTicker;
        }
    }
}