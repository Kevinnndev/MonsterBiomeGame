using System.Collections.Generic;

namespace MonsterBiome.Core.Models
{
    public class BoardState
    {
        public int[,] GridData { get; private set; }
        public bool[,] SolutionCells { get; private set; }
        public int[,] PlacedMonsters { get; private set; }
        public int[,] CellMarks { get; private set; }
        public int[,] ErrorCells { get; private set; }

        public int Rows { get; private set; }
        public int Cols { get; private set; }
        public int PlacedMonstersCount { get; private set; }

        public void Initialize(int[,] parsedGrid, List<(int row, int col)> firstSolution, int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            GridData = parsedGrid;
            PlacedMonstersCount = 0;

            SolutionCells = new bool[rows, cols];
            if (firstSolution != null)
            {
                foreach (var (r, c) in firstSolution)
                {
                    if (r >= 0 && r < rows && c >= 0 && c < cols)
                    {
                        SolutionCells[r, c] = true;
                    }
                }
            }

            PlacedMonsters = new int[rows, cols];
            CellMarks = new int[rows, cols];
            ErrorCells = new int[rows, cols];
        }

        public bool IsInBounds(int r, int c)
        {
            return r >= 0 && r < Rows && c >= 0 && c < Cols;
        }

        public bool IsValidPlacement(int r, int c, int biomeID)
        {
            if (biomeID == 0) return false;
            if (!IsInBounds(r, c)) return false;
            return SolutionCells[r, c];
        }

        public void PlaceMonster(int r, int c)
        {
            if (!IsInBounds(r, c)) return;
            if (PlacedMonsters[r, c] == 0)
            {
                PlacedMonsters[r, c] = 1;
                CellMarks[r, c] = 0;
                PlacedMonstersCount++;
            }
        }

        public void RemoveMonster(int r, int c)
        {
            if (!IsInBounds(r, c)) return;
            if (PlacedMonsters[r, c] == 1)
            {
                PlacedMonsters[r, c] = 0;
                PlacedMonstersCount--;
            }
        }

        public bool ToggleMark(int r, int c)
        {
            if (!IsInBounds(r, c)) return false;
            CellMarks[r, c] = CellMarks[r, c] == 0 ? 1 : 0;
            return CellMarks[r, c] == 1;
        }

        public void MarkError(int r, int c)
        {
            if (IsInBounds(r, c))
            {
                ErrorCells[r, c] = 1;
                CellMarks[r, c] = 0;
            }
        }

        public bool IsErrorCell(int r, int c)
        {
            return IsInBounds(r, c) && ErrorCells[r, c] == 1;
        }

        public bool IsPlacedMonster(int r, int c)
        {
            return IsInBounds(r, c) && PlacedMonsters[r, c] == 1;
        }

        public int CountTotalSolutionCells()
        {
            int count = 0;
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (SolutionCells[r, c]) count++;
                }
            }
            return count;
        }
    }
}
