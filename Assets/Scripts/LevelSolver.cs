using System.Collections.Generic;

/// <summary>
/// Thuật toán DFS Backtracking giải puzzle Monster Biome.
/// Pure C# — không phụ thuộc UnityEditor.
///
/// Luật đặt quái:
///   - Mỗi hàng đặt đúng 1 quái.
///   - Mỗi cột chỉ được dùng 1 lần (toàn board).
///   - Mỗi vùng biome (ID khác 0) chỉ được dùng 1 lần (toàn board).
///   - Quái ở hàng r không được kề (ngang/chéo) với quái ở hàng r-1.
///     Cụ thể: |col[r] - col[r-1]| phải > 1.
/// </summary>
public static class LevelSolver
{
    /// <summary>
    /// Giải puzzle, trả về danh sách các nghiệm tìm được.
    /// Mỗi nghiệm là danh sách (row, col) cho biết quái đặt ở đâu.
    /// Dừng sớm khi đã tìm đủ maxSolutionsToFind nghiệm.
    /// </summary>
    /// <param name="grid">Ma trận biome ID (0 = ô trống, không đặt được).</param>
    /// <param name="rows">Số hàng.</param>
    /// <param name="cols">Số cột.</param>
    /// <param name="maxSolutionsToFind">Số nghiệm tối đa cần tìm (mặc định 2, đủ để biết duy nhất hay không).</param>
    /// <returns>Danh sách nghiệm. Rỗng nếu vô nghiệm hoặc đầu vào không hợp lệ.</returns>
    public static List<List<(int row, int col)>> Solve(int[,] grid, int rows, int cols, int maxSolutionsToFind = 2)
    {
        var results = new List<List<(int row, int col)>>();

        // --- Validate đầu vào ---
        // Board phải vuông (rows == cols)
        if (rows != cols)
        {
            return results;
        }

        // Đếm số vùng biome phân biệt (khác 0) — phải bằng đúng rows
        var distinctBiomes = new HashSet<int>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int biomeId = grid[r, c];
                if (biomeId != 0)
                {
                    distinctBiomes.Add(biomeId);
                }
            }
        }

        if (distinctBiomes.Count != rows)
        {
            return results;
        }

        // --- Chuẩn bị DFS ---
        bool[] usedCols = new bool[cols];
        var usedBiomes = new HashSet<int>();
        var currentSolution = new List<(int row, int col)>();

        DFS(grid, rows, cols, 0, -1, usedCols, usedBiomes, currentSolution, results, maxSolutionsToFind);

        return results;
    }

    /// <summary>
    /// DFS đệ quy theo từng hàng.
    /// </summary>
    /// <param name="currentRow">Hàng đang xét.</param>
    /// <param name="prevCol">Cột đã đặt ở hàng trước (-1 nếu chưa đặt hàng nào).</param>
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
        // Đã đặt xong tất cả các hàng → tìm được 1 nghiệm
        if (currentRow == rows)
        {
            results.Add(new List<(int row, int col)>(currentSolution));
            return;
        }

        for (int c = 0; c < cols; c++)
        {
            // Dừng sớm nếu đã đủ số nghiệm cần tìm
            if (results.Count >= maxSolutionsToFind) return;

            int biomeId = grid[currentRow, c];

            // Pruning: ô trống (biomeId == 0) không đặt được
            if (biomeId == 0) continue;

            // Pruning: cột đã bị dùng bởi hàng khác
            if (usedCols[c]) continue;

            // Pruning: vùng biome đã bị dùng bởi hàng khác
            if (usedBiomes.Contains(biomeId)) continue;

            // Pruning: kề cạnh với quái ở hàng trước (ngang hoặc chéo)
            // |c - prevCol| <= 1 nghĩa là kề → bỏ qua
            if (prevCol >= 0 && System.Math.Abs(c - prevCol) <= 1) continue;

            // Đặt quái tại (currentRow, c)
            usedCols[c] = true;
            usedBiomes.Add(biomeId);
            currentSolution.Add((currentRow, c));

            // Đệ quy sang hàng tiếp theo
            DFS(grid, rows, cols, currentRow + 1, c, usedCols, usedBiomes, currentSolution, results, maxSolutionsToFind);

            // Backtrack
            currentSolution.RemoveAt(currentSolution.Count - 1);
            usedBiomes.Remove(biomeId);
            usedCols[c] = false;
        }
    }
}
