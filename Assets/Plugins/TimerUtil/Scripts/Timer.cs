using System;
using UnityEngine;

namespace TimerUtil
{
    public abstract class Timer
    {
        public string Id { get; }
        public TimerState State { get; private set; }
        public float Seconds => _seconds;
        public static implicit operator float(Timer timer) => timer.Seconds;

        public Action OnStarted;
        public Action OnTicked;
        public Action OnPaused;
        public Action OnResumed;
        public Action OnStopped;
        
        protected float _seconds;
        
        private float _currentStartDelay;
        private Func<bool> _canTickCondition;

        
        public Timer(string id = null)
        {
            Id = id;
            State = TimerState.Stopped;

            TimerService.RegisterTimer(this);
        }

        protected virtual void PrepareStart() {}

        public void Start(float delay = 0f)
        {
            if (State is TimerState.Started or TimerState.StartingDelayed)
                return;

            if (State is TimerState.Paused)
            {
                Resume();
                return;
            }

            PrepareStart();

            if (delay > 0f)
            {
                StartDelayed(delay);
                return;
            }

            SetStarted();
        }

        private void StartDelayed(float delay)
        {
            _currentStartDelay = delay;
            SetState(TimerState.StartingDelayed);
        }

        private void SetStarted()
        {
            SetState(TimerState.Started);
            OnStarted?.Invoke();
        }

        public void Stop()
        {
            if (State is TimerState.Stopped)
                return;

            SetState(TimerState.Stopped);
            OnStopped?.Invoke();
        }

        public void Restart(float delay = 0f)
        {
            Stop();
            Start(delay);
        }

        public void Pause()
        {
            if (State is TimerState.Paused or TimerState.Stopped)
                return;

            SetState(TimerState.Paused);
            OnPaused?.Invoke();
        }

        public void Resume()
        {
            if (State is not TimerState.Paused)
                return;

            SetStarted();
            OnResumed?.Invoke();
        }

        public void SetCanTickCondition(Func<bool> predicate)
        {
            _canTickCondition = predicate;
        }

        public void TryTick()
        {
            if (CanTick())
                ExecuteTick();
        }

        bool CanTick()
        {
            if (State is TimerState.Paused or TimerState.Stopped)
                return false;

            if (_canTickCondition != null && !_canTickCondition())
                return false;

            return true;
        }

        void ExecuteTick()
        {
            if (State is TimerState.StartingDelayed)
            {
                _currentStartDelay -= Time.deltaTime;
                if (_currentStartDelay <= 0f)
                    SetStarted();
                return;
            }

            Tick();
        }

        protected virtual void Tick() {}

        void SetState(TimerState state)
        {
            State = state;
        }
    }
}
