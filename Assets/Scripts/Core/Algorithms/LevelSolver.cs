using System;
using System.Collections.Generic;

namespace MonsterBiome.Core.Algorithms
{
    public static class LevelSolver
    {
        public static List<List<(int row, int col)>> Solve(int[,] grid, int rows, int cols, int maxSolutionsToFind = 2)
        {
            var results = new List<List<(int row, int col)>>();

            if (grid == null || rows <= 0 || cols <= 0 || rows != cols) return results;

            var distinctBiomes = new HashSet<int>();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int biomeId = grid[r, c];
                    if (biomeId != 0) distinctBiomes.Add(biomeId);
                }
            }

            if (distinctBiomes.Count != rows) return results;

            bool[] usedCols = new bool[cols];
            var usedBiomes = new HashSet<int>();
            var currentSolution = new List<(int row, int col)>();

            DFS(grid, rows, cols, 0, -1, usedCols, usedBiomes, currentSolution, results, maxSolutionsToFind);

            return results;
        }

        private static void DFS(
            int[,] grid,
            int rows,
            int cols,
            int currentRow,
            int prevCol,
            bool[] usedCols,
            HashSet<int> usedBiomes,
            List<(int row, int col)> currentSolution,
            List<List<(int row, int col)>> results,
            int maxSolutionsToFind)
        {
            if (currentRow == rows)
            {
                results.Add(new List<(int row, int col)>(currentSolution));
                return;
            }

            for (int c = 0; c < cols; c++)
            {
                if (results.Count >= maxSolutionsToFind) return;

                int biomeId = grid[currentRow, c];
                if (biomeId == 0) continue;
                if (usedCols[c]) continue;
                if (usedBiomes.Contains(biomeId)) continue;
                if (prevCol >= 0 && Math.Abs(c - prevCol) <= 1) continue;

                usedCols[c] = true;
                usedBiomes.Add(biomeId);
                currentSolution.Add((currentRow, c));

                DFS(grid, rows, cols, currentRow + 1, c, usedCols, usedBiomes, currentSolution, results, maxSolutionsToFind);

                currentSolution.RemoveAt(currentSolution.Count - 1);
                usedBiomes.Remove(biomeId);
                usedCols[c] = false;
            }
        }
    }
}
