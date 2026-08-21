using System;

namespace MonsterBiome.Core.Models
{
    public class ScoreCore
    {
        public int CurrentScore { get; private set; } = 0;

        public event Action<int> OnScoreChanged;

        public void ResetScore()
        {
            CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        public void AddScore(int amount)
        {
            CurrentScore += amount;
            if (CurrentScore < 0) CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore);
        }
    }
}
