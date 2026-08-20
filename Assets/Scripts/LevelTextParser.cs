using UnityEngine;
using System.Collections.Generic;
using System;

public static class LevelTextParser
{
    public static int[,] Parse(string textContent, out int rows, out int cols, out bool[,] solutionCells, out int timeLimitSeconds)
    {
        string[] rawLines = textContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<int[]> rowList = new List<int[]>();
        List<bool[]> solutionRowList = new List<bool[]>();
        int? expectedColCount = null;
        
        timeLimitSeconds = -1;

        for (int i = 0; i < rawLines.Length; i++)
        {
            string line = rawLines[i].Trim();

            if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                continue;

            // Xử lý dòng TIME:
            if (line.StartsWith("TIME:", StringComparison.OrdinalIgnoreCase))
            {
                string timeStr = line.Substring(5).Trim();
                if (int.TryParse(timeStr, out int timeVal) && timeVal > 0)
                {
                    timeLimitSeconds = timeVal;
                }
                else
                {
                    throw new Exception($"LỖI PARSE: Dòng TIME không hợp lệ '{line}'. Giá trị thời gian phải là số nguyên dương!");
                }
                continue;
            }

            string[] rawValues = line.Split(new char[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (expectedColCount == null)
            {
                expectedColCount = rawValues.Length;
            }
            else if (rawValues.Length != expectedColCount)
            {
                throw new Exception($"LỖI PARSE: Hàng thứ {i + 1} có {rawValues.Length} cột, nhưng các hàng trước đã có {expectedColCount} cột. Vui lòng sửa lại file text!");
            }

            int[] columns = new int[rawValues.Length];
            bool[] solutionColumns = new bool[rawValues.Length];

            for (int j = 0; j < rawValues.Length; j++)
            {
                string token = rawValues[j];

                // Check for '*' solution marker
                bool isCorrectCell = token.EndsWith("*");
                if (isCorrectCell)
                {
                    token = token.TrimEnd('*');
                }

                if (int.TryParse(token, out int biomeId))
                {
                    if (biomeId < 0)
                    {
                        Debug.LogWarning($"Cảnh báo ở hàng {i + 1}, cột {j + 1}: biomeId '{biomeId}' là số âm, tự chuyển thành ô 0.");
                    }
                    
                    columns[j] = Mathf.Max(0, biomeId);
                    
                    if (columns[j] == 0 && isCorrectCell)
                    {
                        Debug.LogWarning($"Cảnh báo ở hàng {i + 1}, cột {j + 1}: Ô trống (0) được đánh dấu là đáp án (*). Đã tự động huỷ đánh dấu.");
                        solutionColumns[j] = false;
                    }
                    else
                    {
                        solutionColumns[j] = isCorrectCell;
                    }
                }
                else
                {
                    Debug.LogWarning($"Cảnh báo ở hàng {i + 1}, cột {j + 1}: Ký tự '{rawValues[j]}' không hợp lệ, tự chuyển thành ô 0.");
                    columns[j] = 0;
                    solutionColumns[j] = false;
                }
            }

            rowList.Add(columns);
            solutionRowList.Add(solutionColumns);
        }

        if (timeLimitSeconds == -1)
        {
            throw new Exception("LỖI PARSE: Không tìm thấy dòng 'TIME: <số giây>' trong file level!");
        }

        if (rowList.Count == 0)
        {
            throw new Exception("LỖI PARSE: File text rỗng hoặc không có dòng dữ liệu board hợp lệ!");
        }

        rows = rowList.Count;
        cols = expectedColCount.Value;
        int[,] grid = new int[rows, cols];
        solutionCells = new bool[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                grid[r, c] = rowList[r][c];
                solutionCells[r, c] = solutionRowList[r][c];
            }
        }

        return grid;
    }
}
