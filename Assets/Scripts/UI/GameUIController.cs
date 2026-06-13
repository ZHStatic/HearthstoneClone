using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游戏 UI 总控。
/// 负责把 Core 层的状态刷新到 UI，并把玩家点击转换成 GameManager 方法调用。
/// UI 不直接修改手牌、法力、战场和英雄血量。
/// </summary>
public class GameUIController : MonoBehaviour
{
    // 对局核心入口。UI 通过它读取状态和发起操作。
    [SerializeField] private GameManager gameManager;

    // 手牌和双方战场视图。
    [SerializeField] private HandView handView;
    [SerializeField] private BoardView playerBoardView;
    [SerializeField] private BoardView enemyBoardView;

    // HUD 文本：当前玩家、回合、法力、英雄血量、游戏结束提示。
    [SerializeField] private Text currentPlayerText;
    [SerializeField] private Text turnText;
    [SerializeField] private Text manaText;
    [SerializeField] private Text playerHeroText;
    [SerializeField] private Text enemyHeroText;
    [SerializeField] private Text gameOverText;

    // 结束回合按钮。
    [SerializeField] private Button endTurnButton;

    // Unity 生命周期方法：UI 创建后查找 GameManager、注册按钮事件并刷新一次界面。
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

    // 对象销毁时移除按钮监听，避免重复注册或残留引用。
    private void OnDestroy()
    {
        if (endTurnButton != null)
        {
            endTurnButton.onClick.RemoveListener(HandleEndTurnClicked);
        }
    }

    /// <summary>
    /// 刷新整个 UI。
    /// 当前阶段没有事件系统，所以出牌、结束回合后都手动调用它。
    /// </summary>
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

    /// <summary>
    /// 处理手牌点击。
    /// 这里只把操作交给 GameManager，真正的出牌规则仍然在 Core 层。
    /// </summary>
    private void HandleCardClicked(Card card)
    {
        if (gameManager == null) return;

        gameManager.TryPlayMinionCard(card);
        RefreshAll();
    }

    /// <summary>
    /// 处理结束回合按钮点击。
    /// </summary>
    private void HandleEndTurnClicked()
    {
        if (gameManager == null) return;

        gameManager.EndTurn();
        RefreshAll();
    }

    /// <summary>
    /// 刷新当前行动者的手牌。
    /// AI 未完成前，暂时显示 CurrentPlayer 的手牌，方便手动测试双方操作。
    /// </summary>
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

    /// <summary>
    /// 刷新玩家和敌方两边战场。
    /// </summary>
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

    /// <summary>
    /// 刷新当前玩家、回合数、法力、英雄血量和游戏结束提示。
    /// </summary>
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

    /// <summary>
    /// 当 GameManager 不存在时，清空所有 UI 显示。
    /// </summary>
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

    /// <summary>
    /// 刷新单个英雄血量文本。
    /// </summary>
    private void SetHeroText(Text targetText, string label, Player player)
    {
        if (player == null || player.Hero == null)
        {
            SetText(targetText, $"{label}: -");
            return;
        }

        SetText(targetText, $"{label}: {player.Hero.CurrentHealth}/{player.Hero.MaxHealth}");
    }

    /// <summary>
    /// 根据当前胜负状态显示或隐藏游戏结束文本。
    /// </summary>
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

    /// <summary>
    /// 把 Player 对象转换成界面上显示的玩家名称。
    /// </summary>
    private string GetPlayerLabel(Player player)
    {
        if (player == gameManager.Player) return "Player";
        if (player == gameManager.Enemy) return "Enemy";

        return "-";
    }

    /// <summary>
    /// 安全设置 Text 内容。
    /// 如果某个 Text 没有在 Inspector 中绑定，就直接跳过。
    /// </summary>
    private void SetText(Text targetText, string value)
    {
        if (targetText != null)
        {
            targetText.text = value;
        }
    }
}
