using System.Collections.ObjectModel;
using UnityEngine;

namespace TimerUtil
{
    public sealed class TimerTicker : MonoBehaviour
    {
        private ReadOnlyCollection<Timer> _timers;

        void Awake()
        {
            _timers = TimerService.AllTimers;
        }

        public void Update()
        {
            HandleTicking();
        }

        private void HandleTicking()
        {
            for (var i = _timers.Count - 1; i >= 0; --i)
            {
                _timers[i].TryTick();
            }
        }
    }
}