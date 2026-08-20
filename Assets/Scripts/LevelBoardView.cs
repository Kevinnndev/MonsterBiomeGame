using UnityEngine;
using DG.Tweening; 

public class LevelBoardView : MonoBehaviour
{
    [Header("Dữ liệu của màn này")]
    public TextAsset levelTextFile;

    [Header("Giới hạn thời gian")]
    [Tooltip("Thời gian giới hạn (giây) cho màn này. Chỉnh trực tiếp trong Inspector.")]
    public int timeLimitSeconds = 60;

    [Header("Cài đặt hiển thị 2D")]
    [Tooltip("Kích thước ô tối đa (world units). Sẽ bị thu nhỏ tự động nếu grid quá lớn.")]
    public float maxCellSize = 1.2f;
    [Range(0.5f, 1f)]
    [Tooltip("Tỉ lệ padding: board sẽ chiếm bao nhiêu % chiều nhỏ nhất của camera (0.85 = 85%).")]
    public float screenFillRatio = 0.85f;

    public GameObject cellPrefab;

    private BoardCell[] cells;
    private float cellSize; 

    public bool InitializeBoard(GameManager gm, int[,] parsedGrid, int requiredRows, int requiredCols)
    {

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        if (cellPrefab == null)
        {
            Debug.LogError("[LevelBoardView] cellPrefab chưa được gán!");
            return false;
        }

       
        Camera cam = Camera.main;
        if (cam.orthographic)
        {
            float camHeight = cam.orthographicSize * 2f;
            float camWidth  = camHeight * cam.aspect;

            float fitByHeight = (camHeight * screenFillRatio) / requiredRows;
            float fitByWidth  = (camWidth  * screenFillRatio) / requiredCols;

            cellSize = Mathf.Min(fitByHeight, fitByWidth, maxCellSize);
        }
        else
        {
            cellSize = maxCellSize;
        }

        Debug.Log($"[LevelBoardView] Grid {requiredRows}x{requiredCols} → cellSize = {cellSize:F3}");

        int totalCells = requiredRows * requiredCols;
        cells = new BoardCell[totalCells];

        float startX = -(requiredCols - 1) * cellSize / 2f;
        float startY = (requiredRows - 1) * cellSize / 2f;

        for (int r = 0; r < requiredRows; r++)
        {
            for (int c = 0; c < requiredCols; c++)
            {
                int index = (r * requiredCols) + c;

                int biomeID = parsedGrid[r, c];

                Vector3 pos = new Vector3(startX + (c * cellSize), startY - (r * cellSize), 0f);
                GameObject cellObj = Instantiate(cellPrefab, transform.position + pos, Quaternion.identity, transform);


                cellObj.transform.localScale = Vector3.zero;

      
                float targetScale = cellSize / maxCellSize;
                cellObj.transform.DOScale(Vector3.one * targetScale, 0.4f)
                                 .SetEase(Ease.OutBack)
                                 .SetDelay((r + c) * 0.03f);

                BoardCell cell = cellObj.GetComponent<BoardCell>();
                if (cell != null)
                {
                    cells[index] = cell;
                    cell.InitCell(r, c, gm);
                    cell.SetupCell(biomeID, gm.GetBiomeColor(biomeID));
                }
                else
                {
                    Debug.LogError($"[LevelBoardView] cellObj[{r},{c}] thiếu component BoardCell!");
                }
            }
        }

        return true;
    }

    public BoardCell GetCell(int row, int col, int totalCols)
    {
        int index = (row * totalCols) + col;
        if (index < 0 || index >= cells.Length) return null;
        return cells[index];
    }
}