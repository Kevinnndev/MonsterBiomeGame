using System;
using System.Collections.Generic;

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

        // Actions to interface with the View / MoveExecutor / Timer
        public event Action OnAddFreezeTimeRequested;
        public event Action<int, int, int> OnPlaceMonsterRequested;
        public event Action<int, int, int> OnToggleMarkRequested;
        public event Action<Action> OnBoosterAnimationRequested;

        private Func<BoardState> boardStateProvider;

        public void Initialize(Func<BoardState> stateProvider)
        {
            boardStateProvider = stateProvider;
        }

        public void ResetBoosters(int findOne = 1, int freezeTime = 1, int rocket = 1, int bow = 1)
        {
            FindOneCount = Math.Max(1, findOne);
            FreezeTimeCount = Math.Max(1, freezeTime);
            RocketCount = Math.Max(1, rocket);
            BowCount = Math.Max(1, bow);
            ActiveBooster = BoosterType.None;
            OnBoosterCountsChanged?.Invoke();
        }

        public void OnClickFindOne(bool isGameOver)
        {
            if (FindOneCount <= 0 || isGameOver) return;

            BoardState state = boardStateProvider?.Invoke();
            if (state == null) return;

            var allCells = new List<(int, int)>();
            for (int r = 0; r < state.Rows; r++)
                for (int c = 0; c < state.Cols; c++)
                    allCells.Add((r, c));

            if (TryAutoPlaceInScope(state, allCells))
            {
                FindOneCount = Math.Max(0, FindOneCount - 1);
                OnBoosterCountsChanged?.Invoke();
            }
        }

        public void OnClickFreezeTime(bool isGameOver)
        {
            if (FreezeTimeCount <= 0 || isGameOver) return;
            FreezeTimeCount = Math.Max(0, FreezeTimeCount - 1);
            OnBoosterCountsChanged?.Invoke();
            OnAddFreezeTimeRequested?.Invoke();
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

        public void HandleCellClickWithBooster(int targetRow, int targetCol)
        {
            if (ActiveBooster == BoosterType.None) return;

            BoardState state = boardStateProvider?.Invoke();
            if (state == null) return;

            BoosterType usedBooster = ActiveBooster;
            if (usedBooster == BoosterType.Rocket) RocketCount = Math.Max(0, RocketCount - 1);
            else if (usedBooster == BoosterType.Bow) BowCount = Math.Max(0, BowCount - 1);

            ActiveBooster = BoosterType.None;
            OnBoosterCountsChanged?.Invoke();

            var scope = new List<(int, int)>();
            if (usedBooster == BoosterType.Rocket)
            {
                for (int r = 0; r < state.Rows; r++) scope.Add((r, targetCol));
            }
            else if (usedBooster == BoosterType.Bow)
            {
                for (int c = 0; c < state.Cols; c++) scope.Add((targetRow, c));
            }

            (int row, int col)? correctCell = null;
            foreach (var (row, col) in scope)
            {
                if (state.SolutionCells[row, col] && state.PlacedMonsters[row, col] == 0)
                {
                    correctCell = (row, col);
                    break;
                }
            }

            foreach (var (row, col) in scope)
            {
                bool isCorrect = correctCell != null && row == correctCell.Value.row && col == correctCell.Value.col;
                bool isEmpty = state.GridData[row, col] == 0;
                bool alreadyPlaced = state.PlacedMonsters[row, col] == 1;

                if (!isCorrect && !isEmpty && !alreadyPlaced)
                {
                    OnToggleMarkRequested?.Invoke(row, col, state.GridData[row, col]);
                }
            }

            if (correctCell != null)
            {
                var (r, c) = correctCell.Value;
                BoardState capturedState = state;
                
                Action onAnimationComplete = () =>
                {
                    BoardState current = boardStateProvider?.Invoke();
                    if (current == null || current != capturedState) return;
                    TryAutoPlaceInScope(current, new List<(int, int)> { (r, c) });
                };

                OnBoosterAnimationRequested?.Invoke(onAnimationComplete);
            }
        }

        private bool TryAutoPlaceInScope(BoardState state, IEnumerable<(int row, int col)> candidateCells)
        {
            foreach (var (row, col) in candidateCells)
            {
                if (state.SolutionCells[row, col] && state.PlacedMonsters[row, col] == 0)
                {
                    int biomeID = state.GridData[row, col];
                    if (biomeID != 0)
                    {
                        OnPlaceMonsterRequested?.Invoke(row, col, biomeID);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
