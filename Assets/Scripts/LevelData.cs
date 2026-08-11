using UnityEngine;

// Định nghĩa một hàng ngang gồm nhiều cột
[System.Serializable]
public struct BoardRow
{
    public int[] columns;
}

// Dòng này giúp tạo menu chuột phải trên Unity để sinh ra file màn chơi
[CreateAssetMenu(fileName = "Level_", menuName = "Monster Biome/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Bản đồ 5x5 (Nhập ID từ 1 đến 5)")]
    // Tạo 5 hàng ngang để ráp thành lưới 5x5
    public BoardRow[] rows = new BoardRow[5];
}