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

    // 英雄和结束回合按钮。
    // 英雄按钮用于让已选中的随从攻击英雄。
    [SerializeField] private Button playerHeroButton;
    [SerializeField] private Button enemyHeroButton;
    [SerializeField] private Button endTurnButton;

    // 当前被选中的攻击者。
    // 第一次点击己方可攻击随从时设置，攻击或结束回合后清空。
    private Minion selectedAttacker;

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

        if (playerHeroButton != null)
        {
            playerHeroButton.onClick.AddListener(HandlePlayerHeroClicked);
        }

        if (enemyHeroButton != null)
        {
            enemyHeroButton.onClick.AddListener(HandleEnemyHeroClicked);
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

        if (playerHeroButton != null)
        {
            playerHeroButton.onClick.RemoveListener(HandlePlayerHeroClicked);
        }

        if (enemyHeroButton != null)
        {
            enemyHeroButton.onClick.RemoveListener(HandleEnemyHeroClicked);
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

        ClearSelectedAttacker();
        gameManager.TryPlayMinionCard(card);
        RefreshAll();
    }

    /// <summary>
    /// 处理结束回合按钮点击。
    /// </summary>
    private void HandleEndTurnClicked()
    {
        if (gameManager == null) return;

        ClearSelectedAttacker();
        gameManager.EndTurn();
        RefreshAll();
    }

    /// <summary>
    /// 处理玩家英雄被点击。
    /// 如果当前已经选中攻击者，就尝试攻击玩家英雄。
    /// </summary>
    private void HandlePlayerHeroClicked()
    {
        if (gameManager == null) return;
        if (gameManager.Player == null) return;

        TryAttackSelectedHero(gameManager.Player.Hero);
        RefreshAll();
    }

    /// <summary>
    /// 处理敌方英雄被点击。
    /// 如果当前已经选中攻击者，就尝试攻击敌方英雄。
    /// </summary>
    private void HandleEnemyHeroClicked()
    {
        if (gameManager == null) return;
        if (gameManager.Enemy == null) return;

        TryAttackSelectedHero(gameManager.Enemy.Hero);
        RefreshAll();
    }

    /// <summary>
    /// 处理随从点击。
    /// 没有攻击者时，尝试选择当前玩家的可攻击随从。
    /// 已经有攻击者时，点击敌方随从会尝试攻击。
    /// </summary>
    private void HandleMinionClicked(Minion clickedMinion)
    {
        if (gameManager == null) return;
        if (clickedMinion == null) return;
        if (gameManager.IsGameOver) return;

        if (selectedAttacker == null)
        {
            SelectAttacker(clickedMinion);
            RefreshAll();
            return;
        }

        if (clickedMinion == selectedAttacker)
        {
            ClearSelectedAttacker();
            RefreshAll();
            return;
        }

        if (clickedMinion.Owner == selectedAttacker.Owner)
        {
            SelectAttacker(clickedMinion);
            RefreshAll();
            return;
        }

        TryAttackSelectedTarget(clickedMinion);
        RefreshAll();
    }

    /// <summary>
    /// 尝试把某个随从设置为当前攻击者。
    /// 只允许选择当前玩家、可攻击、未死亡的随从。
    /// </summary>
    private void SelectAttacker(Minion minion)
    {
        if (minion == null)
        {
            ClearSelectedAttacker();
            return;
        }

        if (minion.Owner != gameManager.CurrentPlayer)
        {
            ClearSelectedAttacker();
            return;
        }

        if (!minion.CanAttack || minion.IsDead)
        {
            ClearSelectedAttacker();
            return;
        }

        selectedAttacker = minion;
    }

    /// <summary>
    /// 使用当前选中的攻击者，尝试攻击目标随从。
    /// 攻击规则仍然交给 GameManager.TryAttackMinion 判断。
    /// </summary>
    private void TryAttackSelectedTarget(Minion target)
    {
        if (selectedAttacker == null) return;
        if (target == null) return;

        gameManager.TryAttackMinion(selectedAttacker, target);
        ClearSelectedAttacker();
    }

    /// <summary>
    /// 使用当前选中的攻击者，尝试攻击目标英雄。
    /// 是否能攻击自己的英雄、是否是合法目标，仍然交给 GameManager 判断。
    /// </summary>
    private void TryAttackSelectedHero(Hero targetHero)
    {
        if (selectedAttacker == null) return;
        if (targetHero == null) return;

        gameManager.TryAttackHero(selectedAttacker, targetHero);
        ClearSelectedAttacker();
    }

    /// <summary>
    /// 清空当前选中的攻击者。
    /// </summary>
    private void ClearSelectedAttacker()
    {
        selectedAttacker = null;
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
            playerBoardView.Refresh(gameManager.Board.GetMinions(gameManager.Player), HandleMinionClicked);
        }

        if (enemyBoardView != null)
        {
            enemyBoardView.Refresh(gameManager.Board.GetMinions(gameManager.Enemy), HandleMinionClicked);
        }
    }

    /// <summary>
    /// 刷新当前玩家、回合数、法力、英雄血量和游戏结束提示。
    /// </summary>
    private void RefreshHud()
    {
        Player currentPlayer = gameManager.CurrentPlayer;

        SetText(currentPlayerText, GetCurrentPlayerText(currentPlayer));
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

        if (playerHeroButton != null)
        {
            playerHeroButton.interactable = !gameManager.IsGameOver;
        }

        if (enemyHeroButton != null)
        {
            enemyHeroButton.interactable = !gameManager.IsGameOver;
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
    /// 生成当前玩家文本。
    /// 如果已经选中攻击者，顺便显示选中的随从名，方便调试点击流程。
    /// </summary>
    private string GetCurrentPlayerText(Player currentPlayer)
    {
        string text = $"Current: {GetPlayerLabel(currentPlayer)}";

        if (selectedAttacker != null && selectedAttacker.CardData != null)
        {
            text += $" | Selected: {selectedAttacker.CardData.CardName}";
        }

        return text;
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
