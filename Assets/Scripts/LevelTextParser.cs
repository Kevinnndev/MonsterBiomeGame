using UnityEngine;
using System.Collections.Generic;
using System;

public static class LevelTextParser
{
    /// <summary>
    /// Parse file text level thành ma trận biome ID.
    /// Đã bỏ hoàn toàn output solutionCells — đáp án giờ được tính bởi LevelSolver lúc runtime.
    /// Nếu file cũ vẫn còn dấu '*' sót lại (ví dụ "3*"), parser sẽ tự bỏ qua an toàn
    /// (coi "3*" tương đương "3") để không phải sửa tay xoá '*' khỏi từng file.
    /// </summary>
    public static int[,] Parse(string textContent, out int rows, out int cols, out int timeLimitSeconds)
    {
        string[] rawLines = textContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<int[]> rowList = new List<int[]>();
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

            for (int j = 0; j < rawValues.Length; j++)
            {
                string token = rawValues[j];

                // Bỏ qua dấu '*' sót lại từ file cũ — coi "3*" tương đương "3"
                // Chỉ strip, không dùng kết quả isCorrectCell cho việc gì nữa
                if (token.EndsWith("*"))
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
                }
                else
                {
                    Debug.LogWarning($"Cảnh báo ở hàng {i + 1}, cột {j + 1}: Ký tự '{rawValues[j]}' không hợp lệ, tự chuyển thành ô 0.");
                    columns[j] = 0;
                }
            }

            rowList.Add(columns);
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
