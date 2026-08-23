using System;
using TimerUtil;

public static class TimerExtensions
{
    public static CountdownTimer DestroyOnComplete(this CountdownTimer countdownTimer)
    {
        countdownTimer.DestroyOnComplete = true;
        return countdownTimer;
    }

    public static CountdownTimer Duration(this CountdownTimer countdownTimer, float duration)
    {
        countdownTimer.SetDuration(duration);
        return countdownTimer;
    }

    public static CountdownTimer LoopCount(this CountdownTimer countdownTimer, int loopCount)
    {
        countdownTimer.SetLoopCount(loopCount);
        return countdownTimer;
    }

    public static CountdownTimer OnComplete(this CountdownTimer countdownTimer, Action onComplete)
    {
        countdownTimer.OnCountdownComplete += onComplete;
        return countdownTimer;
    }
    
    public static CountdownTimer OnLoop(this CountdownTimer countdownTimer, Action onLoopComplete)
    {
        countdownTimer.OnLoopComplete += onLoopComplete;
        return countdownTimer;
    }

    public static CountdownTimer OnStart(this CountdownTimer timer, Action onStart)
    {
        timer.OnStarted += onStart;
        return timer;
    }
    
    public static CountdownTimer OnTick(this CountdownTimer timer, Action onTicked)
    {
        timer.OnTicked += onTicked;
        return timer;
    }
    
    public static CountdownTimer OnPause(this CountdownTimer timer, Action onPaused)
    {
        timer.OnPaused += onPaused;
        return timer;
    }
    
    public static CountdownTimer OnResume(this CountdownTimer timer, Action onResumed)
    {
        timer.OnResumed += onResumed;
        return timer;
    }
    
    public static CountdownTimer OnStop(this CountdownTimer timer, Action onStopped)
    {
        timer.OnStopped += onStopped;
        return timer;
    }
    
    public static CountdownTimer CanTickCondition(this CountdownTimer timer, Func<bool> condition)
    {
        timer.SetCanTickCondition(condition);
        return timer;
    }
}
