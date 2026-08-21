using System;

namespace MonsterBiome.Core.Models
{
    public class LivesCore
    {
        public int Lives { get; private set; } = 3;

        public event Action<int> OnLivesChanged;
        public event Action OnLivesDepleted;

        public void ResetLives(int initialLives = 3)
        {
            Lives = initialLives;
            OnLivesChanged?.Invoke(Lives);
        }

        public void DeductLife()
        {
            Lives--;
            if (Lives < 0) Lives = 0;
            OnLivesChanged?.Invoke(Lives);

            if (Lives <= 0)
            {
                OnLivesDepleted?.Invoke();
            }
        }
    }
}
