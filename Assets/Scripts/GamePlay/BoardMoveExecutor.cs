using UnityEngine;
using MonsterBiome.Core.Models;

public class BoardMoveExecutor : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GameTheme theme;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private LivesManager livesManager;
    [SerializeField] private SettingsController settingsController;

    private System.Func<BoardState> boardStateProvider;
    private System.Func<LevelBoardView> boardViewProvider;

    public event System.Action OnBoardCompleted;

    private BoardState BoardState => boardStateProvider?.Invoke();

    public void Initialize(System.Func<BoardState> stateProvider, System.Func<LevelBoardView> viewProvider,
        GameTheme gameTheme, AudioManager audio, ScoreManager score, LivesManager lives, SettingsController settings)
    {
        boardStateProvider = stateProvider;
        boardViewProvider = viewProvider;
        theme = gameTheme;
        audioManager = audio;
        scoreManager = score;
        livesManager = lives;
        settingsController = settings;
    }

    private BoardCell GetCellOrNull(int row, int col, int cols)
    {
        LevelBoardView view = boardViewProvider?.Invoke();
        return view != null ? view.GetCell(row, col, cols) : null;
    }

    public void ToggleMark(int row, int col, int biomeID)
    {
        BoardState state = BoardState;
        if (state == null) return;

        BoardCell targetCell = GetCellOrNull(row, col, state.Cols);
        bool isMarked = state.ToggleMark(row, col);
        if (targetCell != null)
        {
            targetCell.SetMarkState(isMarked, GetBiomeColor(biomeID), theme.markedCellAlpha);
        }
    }

    public void TryPlaceMonster(int row, int col, int biomeID)
    {
        BoardState state = BoardState;
        if (state == null) return;

        BoardCell targetCell = GetCellOrNull(row, col, state.Cols);

        if (state.IsValidPlacement(row, col, biomeID))
        {
            PlaceMonsterAt(row, col, biomeID);
        }
        else
        {
            livesManager.DeductLife();
            audioManager.PlayError();
            if (!Application.isEditor && !settingsController.IsVibrationOff) Handheld.Vibrate();

            if (targetCell != null)
            {
                targetCell.ShowErrorSprite(theme.brokenHeartSprite);
            }
            state.MarkError(row, col);
        }
    }

    public void PlaceMonsterAt(int row, int col, int biomeID)
    {
        BoardState state = BoardState;
        if (state == null) return;

        state.PlaceMonster(row, col);

        BoardCell targetCell = GetCellOrNull(row, col, state.Cols);
        if (targetCell != null)
        {
            targetCell.SetMonsterState(true, GetMonsterSprite(biomeID), GetBiomeColor(biomeID));
        }

        scoreManager.AddScore(theme.scorePerMonster);
        audioManager.PlayPlaceMonster();

        if (state.PlacedMonstersCount >= state.CountTotalSolutionCells())
        {
            OnBoardCompleted?.Invoke();
        }
    }

    public void RemoveMonster(int row, int col)
    {
        BoardState state = BoardState;
        if (state == null) return;

        state.RemoveMonster(row, col);

        BoardCell targetCell = GetCellOrNull(row, col, state.Cols);
        if (targetCell != null)
        {
            targetCell.SetMonsterState(false, null, Color.white);
        }
    }

    private Color GetBiomeColor(int biomeID) => theme.GetBiomeColor(biomeID);
    private Sprite GetMonsterSprite(int biomeID) => theme.GetMonsterSprite(biomeID);
}
