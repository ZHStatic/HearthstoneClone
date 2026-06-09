using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Connects the UI layer to GameManager.
/// UI reads state from Core and sends player actions back through GameManager.
/// </summary>
public class GameUIController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private HandView handView;
    [SerializeField] private BoardView playerBoardView;
    [SerializeField] private BoardView enemyBoardView;
    [SerializeField] private Text currentPlayerText;
    [SerializeField] private Text turnText;
    [SerializeField] private Text manaText;
    [SerializeField] private Text playerHeroText;
    [SerializeField] private Text enemyHeroText;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Button endTurnButton;

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(HandleEndTurnClicked);
        }

        RefreshAll();
    }

    private void OnDestroy()
    {
        if (endTurnButton != null)
        {
            endTurnButton.onClick.RemoveListener(HandleEndTurnClicked);
        }
    }

    public void RefreshAll()
    {
        if (gameManager == null)
        {
            ClearAll();
            return;
        }

        RefreshHand();
        RefreshBoards();
        RefreshHud();
    }

    private void HandleCardClicked(Card card)
    {
        if (gameManager == null) return;

        gameManager.TryPlayMinionCard(card);
        RefreshAll();
    }

    private void HandleEndTurnClicked()
    {
        if (gameManager == null) return;

        gameManager.EndTurn();
        RefreshAll();
    }

    private void RefreshHand()
    {
        if (handView == null) return;

        Player currentPlayer = gameManager.CurrentPlayer;
        if (currentPlayer == null)
        {
            handView.Clear();
            return;
        }

        handView.SetHand(currentPlayer.Hand, HandleCardClicked);
    }

    private void RefreshBoards()
    {
        if (gameManager.Board == null) return;

        if (playerBoardView != null)
        {
            playerBoardView.Refresh(gameManager.Board.GetMinions(gameManager.Player));
        }

        if (enemyBoardView != null)
        {
            enemyBoardView.Refresh(gameManager.Board.GetMinions(gameManager.Enemy));
        }
    }

    private void RefreshHud()
    {
        Player currentPlayer = gameManager.CurrentPlayer;

        SetText(currentPlayerText, $"Current: {GetPlayerLabel(currentPlayer)}");
        SetText(turnText, $"Turn: {gameManager.TurnNumber}");

        if (currentPlayer != null)
        {
            SetText(manaText, $"Mana: {currentPlayer.CurrentMana}/{currentPlayer.MaxMana}");
        }
        else
        {
            SetText(manaText, "Mana: -");
        }

        SetHeroText(playerHeroText, "Player", gameManager.Player);
        SetHeroText(enemyHeroText, "Enemy", gameManager.Enemy);
        SetGameOverText();

        if (endTurnButton != null)
        {
            endTurnButton.interactable = !gameManager.IsGameOver;
        }
    }

    private void ClearAll()
    {
        if (handView != null)
        {
            handView.Clear();
        }

        if (playerBoardView != null)
        {
            playerBoardView.Clear();
        }

        if (enemyBoardView != null)
        {
            enemyBoardView.Clear();
        }

        SetText(currentPlayerText, "Current: -");
        SetText(turnText, "Turn: -");
        SetText(manaText, "Mana: -");
        SetText(playerHeroText, "Player: -");
        SetText(enemyHeroText, "Enemy: -");
        SetText(gameOverText, "");
    }

    private void SetHeroText(Text targetText, string label, Player player)
    {
        if (player == null || player.Hero == null)
        {
            SetText(targetText, $"{label}: -");
            return;
        }

        SetText(targetText, $"{label}: {player.Hero.CurrentHealth}/{player.Hero.MaxHealth}");
    }

    private void SetGameOverText()
    {
        if (gameOverText == null) return;

        if (!gameManager.IsGameOver)
        {
            gameOverText.text = "";
            gameOverText.gameObject.SetActive(false);
            return;
        }

        gameOverText.gameObject.SetActive(true);

        if (gameManager.Winner == null)
        {
            gameOverText.text = "Game Over: Draw";
            return;
        }

        gameOverText.text = $"Game Over: {GetPlayerLabel(gameManager.Winner)} wins";
    }

    private string GetPlayerLabel(Player player)
    {
        if (player == gameManager.Player) return "Player";
        if (player == gameManager.Enemy) return "Enemy";

        return "-";
    }

    private void SetText(Text targetText, string value)
    {
        if (targetText != null)
        {
            targetText.text = value;
        }
    }
}
