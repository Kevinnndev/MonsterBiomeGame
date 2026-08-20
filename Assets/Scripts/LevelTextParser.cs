using UnityEngine;
using System.Collections.Generic;
using System;

public static class LevelTextParser
{
    public static int[,] Parse(string textContent, out int rows, out int cols)
    {
        string[] rawLines = textContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<int[]> rowList = new List<int[]>();
        int? expectedColCount = null;

        for (int i = 0; i < rawLines.Length; i++)
        {
            string line = rawLines[i].Trim();

            if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("TIME:", StringComparison.OrdinalIgnoreCase))
                continue;

            string[] rawValues = line.Split(new char[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (expectedColCount == null)
            {
                expectedColCount = rawValues.Length;
            }
            else if (rawValues.Length != expectedColCount)
            {
                throw new Exception($"Lỗi Parse: Hàng thứ {i + 1} có {rawValues.Length} cột, khác với số cột {expectedColCount} trước đó.");
            }

            int[] columns = new int[rawValues.Length];

            for (int j = 0; j < rawValues.Length; j++)
            {
                string token = rawValues[j];

                if (token.EndsWith("*")) token = token.TrimEnd('*');

                if (int.TryParse(token, out int biomeId))
                {
                    columns[j] = Mathf.Max(0, biomeId);
                }
                else
                {
                    columns[j] = 0;
                }
            }

            rowList.Add(columns);
        }

        if (rowList.Count == 0)
        {
            throw new Exception("File text rỗng hoặc không có dữ liệu hợp lệ.");
        }

        rows = rowList.Count;
        cols = expectedColCount.Value;
        int[,] grid = new int[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                grid[r, c] = rowList[r][c];
            }
        }

        return grid;
    }
}
