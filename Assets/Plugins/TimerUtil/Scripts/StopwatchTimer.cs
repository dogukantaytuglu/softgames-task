using UnityEngine;

namespace TimerUtil
{
    public class StopwatchTimer : Timer
    {
        public StopwatchTimer(string id = null) : base(id)
        {
        }

        protected override void PrepareStart()
        {
            _seconds = 0;
        }
        protected override void Tick()
        {
            _seconds += Time.deltaTime;
            OnTicked?.Invoke();
        }
    }
}
