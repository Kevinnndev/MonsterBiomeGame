using System;

namespace MonsterBiome.Core.Models
{
    public enum BoosterType { None, FindOne, FreezeTime, Rocket, Bow }

    public class BoosterCore
    {
        public int FindOneCount { get; private set; } = 1;
        public int FreezeTimeCount { get; private set; } = 1;
        public int RocketCount { get; private set; } = 1;
        public int BowCount { get; private set; } = 1;

        public BoosterType ActiveBooster { get; private set; } = BoosterType.None;

        public event Action OnBoosterCountsChanged;
        public event Action OnFindOneRequested;
        public event Action OnFreezeTimeRequested;
        public event Action<int, int, BoosterType> OnBoosterTargetClicked;

        public void ResetBoosters(int findOne = 1, int freezeTime = 1, int rocket = 1, int bow = 1)
        {
            FindOneCount = findOne > 0 ? findOne : 1;
            FreezeTimeCount = freezeTime > 0 ? freezeTime : 1;
            RocketCount = rocket > 0 ? rocket : 1;
            BowCount = bow > 0 ? bow : 1;
            ActiveBooster = BoosterType.None;
            OnBoosterCountsChanged?.Invoke();
        }

        public void OnClickFindOne(bool isGameOver)
        {
            if (FindOneCount <= 0 || isGameOver) return;
            OnFindOneRequested?.Invoke();
        }

        public void ConsumeFindOne()
        {
            FindOneCount--;
            if (FindOneCount < 0) FindOneCount = 0;
            OnBoosterCountsChanged?.Invoke();
        }

        public void OnClickFreezeTime(bool isGameOver)
        {
            if (FreezeTimeCount <= 0 || isGameOver) return;
            FreezeTimeCount--;
            if (FreezeTimeCount < 0) FreezeTimeCount = 0;
            OnFreezeTimeRequested?.Invoke();
            OnBoosterCountsChanged?.Invoke();
        }

        public void OnClickRocket(bool isGameOver)
        {
            if (RocketCount <= 0 || isGameOver) return;
            ActiveBooster = BoosterType.Rocket;
        }

        public void OnClickBow(bool isGameOver)
        {
            if (BowCount <= 0 || isGameOver) return;
            ActiveBooster = BoosterType.Bow;
        }

        public void HandleCellClickWithBooster(int row, int col)
        {
            if (ActiveBooster == BoosterType.None) return;

            BoosterType usedBooster = ActiveBooster;
            if (usedBooster == BoosterType.Rocket) RocketCount--;
            else if (usedBooster == BoosterType.Bow) BowCount--;

            if (RocketCount < 0) RocketCount = 0;
            if (BowCount < 0) BowCount = 0;

            ActiveBooster = BoosterType.None;
            OnBoosterCountsChanged?.Invoke();

            OnBoosterTargetClicked?.Invoke(row, col, usedBooster);
        }

        public void ClearActiveBooster()
        {
            ActiveBooster = BoosterType.None;
        }
    }
}
