using UnityEngine;
using TMPro;

public class MovesManager : MonoBehaviour
{
    [Header("▶ UI Reference")]
    [Tooltip("Canvas/MainPanel/MovesBackground/Moves objesindeki TMP bileşeni")]
    public TextMeshProUGUI movesText;

    private GameBoard board;
    private int remainingMoves;

    private void Awake()
    {
        board = FindFirstObjectByType<GameBoard>();
        if (board == null)
            Debug.LogError("GameBoard bulunamadı!");
    }

    private void Start()
    {
        remainingMoves = board.maxMoves;
        UpdateMovesUI();
    }
    public int GetRemainingMoves()
    {
        return remainingMoves;
    }
    public void UseMove()
    {
        if (remainingMoves <= 0) return;

        remainingMoves--;
        UpdateMovesUI();

        if (remainingMoves == 0)
        {
            Debug.Log("Hamle hakkı bitti");
            FindFirstObjectByType<VictoryPanelController>().ShowVictory();
        }
    }

    private void UpdateMovesUI()
    {
        if (movesText != null)
            movesText.text = remainingMoves.ToString();
    }
}