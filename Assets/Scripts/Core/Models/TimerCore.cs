using System;

namespace MonsterBiome.Core.Models
{
    public class TimerCore
    {
        public float CurrentTime { get; private set; }
        public float FreezeTimeRemaining { get; private set; }
        public bool IsTimerRunning { get; private set; }
        public bool IsPaused { get; private set; }

        public event Action OnTimerExpired;
        public event Action<float, bool> OnTimerTick; // (currentTime, isFrozen)

        public void StartTimer(float durationSeconds)
        {
            CurrentTime = durationSeconds;
            FreezeTimeRemaining = 0f;
            IsPaused = false;
            IsTimerRunning = true;
            OnTimerTick?.Invoke(CurrentTime, false);
        }

        public void StopTimer()
        {
            IsTimerRunning = false;
        }

        public void PauseTimer()
        {
            IsPaused = true;
        }

        public void ResumeTimer()
        {
            IsPaused = false;
        }

        public void AddTime(float seconds)
        {
            CurrentTime += seconds;
            if (!IsTimerRunning)
            {
                IsTimerRunning = true;
                IsPaused = false;
            }
            OnTimerTick?.Invoke(CurrentTime, FreezeTimeRemaining > 0f);
        }

        public void AddFreezeTime(float seconds)
        {
            FreezeTimeRemaining += seconds;
            OnTimerTick?.Invoke(CurrentTime, true);
        }

        public void Tick(float deltaTime)
        {
            if (IsPaused) return;

            if (FreezeTimeRemaining > 0f)
            {
                FreezeTimeRemaining -= deltaTime;
                if (FreezeTimeRemaining < 0f) FreezeTimeRemaining = 0f;
                OnTimerTick?.Invoke(CurrentTime, true);
                return;
            }

            if (!IsTimerRunning) return;

            CurrentTime -= deltaTime;

            if (CurrentTime <= 0f)
            {
                CurrentTime = 0f;
                IsTimerRunning = false;
                OnTimerTick?.Invoke(0f, false);
                OnTimerExpired?.Invoke();
            }
            else
            {
                OnTimerTick?.Invoke(CurrentTime, false);
            }
        }
    }
}
