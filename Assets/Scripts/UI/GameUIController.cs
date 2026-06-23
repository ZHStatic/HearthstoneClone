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

    // 当前被选中的法术牌。
    // 点击法术牌时设置，点击目标、结束回合或改打其他牌后清空。
    private Card selectedSpellCard;

    private string feedbackMessage = "";

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
        ClearSelectedSpell();

        if (IsSpellCard(card))
        {
            SelectSpellCardForTargeting(card);
            RefreshAll();
            return;
        }

        if (IsUnsupportedCardType(card))
        {
            SetFeedback("当前阶段暂不支持这种卡牌类型。");
            RefreshAll();
            return;
        }

        GameActionResult result = gameManager.TryPlayMinionCardDetailed(card);
        SetFeedback(GetActionResultMessageOrFallback(
            result,
            $"打出 {GetCardName(card)}。",
            "出牌失败。"));
        RefreshAll();
    }

    /// <summary>
    /// 选择一张法术牌，等待玩家点击目标。
    /// 法术详细结果还没接入前，这里暂时保留法术选择所需的 UI 前置检查。
    /// </summary>
    private void SelectSpellCardForTargeting(Card card)
    {
        if (gameManager.IsGameOver)
        {
            SetFeedback("游戏已经结束，不能继续出牌。");
            return;
        }

        if (card == null || card.CardData == null)
        {
            SetFeedback("这张卡的数据无效。");
            return;
        }

        Player currentPlayer = gameManager.CurrentPlayer;
        if (currentPlayer == null)
        {
            SetFeedback("当前没有行动玩家。");
            return;
        }

        if (!currentPlayer.HasCardInHand(card))
        {
            SetFeedback("这张卡不在当前玩家手牌里。");
            return;
        }

        if (card.CurrentCost > currentPlayer.CurrentMana)
        {
            SetFeedback($"{card.CardData.CardName} 需要 {card.CurrentCost} 点法力，当前只有 {currentPlayer.CurrentMana} 点。");
            return;
        }

        selectedSpellCard = card;
        SetFeedback($"已选择 {card.CardData.CardName}，请选择法术目标。");
    }

    /// <summary>
    /// 处理结束回合按钮点击。
    /// </summary>
    private void HandleEndTurnClicked()
    {
        if (gameManager == null) return;

        if (gameManager.IsGameOver)
        {
            SetFeedback("游戏已经结束，不能结束回合。");
            RefreshAll();
            return;
        }

        ClearSelectedAttacker();
        ClearSelectedSpell();
        gameManager.EndTurn();
        SetFeedback($"{GetPlayerLabel(gameManager.CurrentPlayer)} 回合开始。");
        RefreshAll();
    }

    /// <summary>
    /// 处理玩家英雄被点击。
    /// 如果当前已经选中攻击者，就尝试攻击玩家英雄。
    /// </summary>
    private void HandlePlayerHeroClicked()
    {
        if (gameManager == null) return;
        if (gameManager.Player == null)
        {
            SetFeedback("玩家英雄不存在。");
            RefreshAll();
            return;
        }

        if (selectedSpellCard != null)
        {
            TryPlaySelectedSpellOnHero(gameManager.Player.Hero);
        }
        else
        {
            TryAttackSelectedHero(gameManager.Player.Hero);
        }

        RefreshAll();
    }

    /// <summary>
    /// 处理敌方英雄被点击。
    /// 如果当前已经选中攻击者，就尝试攻击敌方英雄。
    /// </summary>
    private void HandleEnemyHeroClicked()
    {
        if (gameManager == null) return;
        if (gameManager.Enemy == null)
        {
            SetFeedback("敌方英雄不存在。");
            RefreshAll();
            return;
        }

        if (selectedSpellCard != null)
        {
            TryPlaySelectedSpellOnHero(gameManager.Enemy.Hero);
        }
        else
        {
            TryAttackSelectedHero(gameManager.Enemy.Hero);
        }

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
        if (clickedMinion == null)
        {
            SetFeedback("没有点击到有效随从。");
            RefreshAll();
            return;
        }

        if (gameManager.IsGameOver)
        {
            SetFeedback("游戏已经结束，不能继续操作。");
            RefreshAll();
            return;
        }

        if (selectedSpellCard != null)
        {
            TryPlaySelectedSpellOnMinion(clickedMinion);
            RefreshAll();
            return;
        }

        if (selectedAttacker == null)
        {
            SelectAttacker(clickedMinion);
            RefreshAll();
            return;
        }

        if (clickedMinion == selectedAttacker)
        {
            SetFeedback($"取消选中 {GetMinionName(clickedMinion)}。");
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
            SetFeedback("没有选中有效随从。");
            return;
        }

        if (gameManager.CurrentPlayer == null)
        {
            ClearSelectedAttacker();
            SetFeedback("当前没有行动玩家。");
            return;
        }

        if (minion.Owner != gameManager.CurrentPlayer)
        {
            ClearSelectedAttacker();
            SetFeedback("只能选择当前玩家自己的随从。");
            return;
        }

        if (minion.IsDead)
        {
            ClearSelectedAttacker();
            SetFeedback($"{GetMinionName(minion)} 已经死亡，不能攻击。");
            return;
        }

        if (!minion.CanAttack)
        {
            ClearSelectedAttacker();
            SetFeedback($"{GetMinionName(minion)} 现在不能攻击。");
            return;
        }

        selectedAttacker = minion;
        SetFeedback($"已选中 {GetMinionName(minion)}，请选择攻击目标。");
    }

    /// <summary>
    /// 使用当前选中的攻击者，尝试攻击目标随从。
    /// 攻击规则仍然交给 GameManager.TryAttackMinion 判断。
    /// </summary>
    private void TryAttackSelectedTarget(Minion target)
    {
        if (selectedAttacker == null)
        {
            SetFeedback("请先选择一个可以攻击的随从。");
            return;
        }

        if (target == null)
        {
            SetFeedback("攻击目标无效。");
            return;
        }

        string attackerName = GetMinionName(selectedAttacker);
        string targetName = GetMinionName(target);
        bool attacked = gameManager.TryAttackMinion(selectedAttacker, target);
        SetFeedback(attacked ? $"{attackerName} 攻击 {targetName}。" : "目标非法，攻击失败。");
        ClearSelectedAttacker();
    }

    /// <summary>
    /// 使用当前选中的攻击者，尝试攻击目标英雄。
    /// 是否能攻击自己的英雄、是否是合法目标，仍然交给 GameManager 判断。
    /// </summary>
    private void TryAttackSelectedHero(Hero targetHero)
    {
        if (gameManager == null) return;

        if (gameManager.IsGameOver)
        {
            SetFeedback("游戏已经结束，不能继续攻击。");
            return;
        }

        if (selectedAttacker == null)
        {
            SetFeedback("请先选择一个可以攻击的随从。");
            return;
        }

        if (targetHero == null)
        {
            SetFeedback("攻击目标英雄无效。");
            return;
        }

        string attackerName = GetMinionName(selectedAttacker);
        bool attacked = gameManager.TryAttackHero(selectedAttacker, targetHero);
        SetFeedback(attacked ? $"{attackerName} 攻击 {targetHero.Name}。" : "目标英雄非法，攻击失败。");
        ClearSelectedAttacker();
    }

    /// <summary>
    /// 使用当前选中的法术牌，尝试对目标随从释放法术。
    /// </summary>
    private void TryPlaySelectedSpellOnMinion(Minion target)
    {
        if (selectedSpellCard == null)
        {
            SetFeedback("请先选择一张法术牌。");
            return;
        }

        if (gameManager.IsGameOver)
        {
            SetFeedback("游戏已经结束，不能继续施放法术。");
            ClearSelectedSpell();
            return;
        }

        if (target == null)
        {
            SetFeedback("法术目标无效。");
            return;
        }

        string spellName = GetCardName(selectedSpellCard);
        string targetName = GetMinionName(target);

        GameActionResult result = gameManager.TryPlaySpellCardOnMinionDetailed(selectedSpellCard, target);
        string fallbackMessage = $"{spellName} 对 {targetName} 释放成功。";
        SetFeedback(GetActionResultMessageOrFallback(
            result,
            fallbackMessage,
            "法术目标非法，释放失败。"));
        ClearSelectedSpell();
        ClearSelectedAttacker();
    }

    /// <summary>
    /// 使用当前选中的法术牌，尝试对目标英雄释放法术。
    /// </summary>
    private void TryPlaySelectedSpellOnHero(Hero targetHero)
    {
        if (selectedSpellCard == null)
        {
            SetFeedback("请先选择一张法术牌。");
            return;
        }

        if (gameManager.IsGameOver)
        {
            SetFeedback("游戏已经结束，不能继续施放法术。");
            ClearSelectedSpell();
            return;
        }

        if (targetHero == null)
        {
            SetFeedback("法术目标英雄无效。");
            return;
        }

        string spellName = GetCardName(selectedSpellCard);
        string targetName = targetHero.Name;

        GameActionResult result = gameManager.TryPlaySpellCardOnHeroDetailed(selectedSpellCard, targetHero);
        string fallbackMessage = $"{spellName} 对 {targetName} 释放成功。";
        SetFeedback(GetActionResultMessageOrFallback(
            result,
            fallbackMessage,
            "法术目标英雄非法，释放失败。"));
        ClearSelectedSpell();
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
    /// 清空当前选中的法术牌。
    /// </summary>
    private void ClearSelectedSpell()
    {
        selectedSpellCard = null;
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
            playerBoardView.Refresh(gameManager.Board.GetMinions(gameManager.Player), HandleMinionClicked, selectedAttacker);
        }

        if (enemyBoardView != null)
        {
            enemyBoardView.Refresh(gameManager.Board.GetMinions(gameManager.Enemy), HandleMinionClicked, selectedAttacker);
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
        ClearSelectedAttacker();
        ClearSelectedSpell();
        ClearFeedback();

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

        if (selectedSpellCard != null && selectedSpellCard.CardData != null)
        {
            text += $" | Spell: {selectedSpellCard.CardData.CardName}";
        }

        return text;
    }

    /// <summary>
    /// 安全获取随从名称，避免提示文本因为空数据报错。
    /// </summary>
    private string GetMinionName(Minion minion)
    {
        if (minion == null || minion.CardData == null)
        {
            return "未知随从";
        }

        return minion.CardData.CardName;
    }

    /// <summary>
    /// 安全获取卡牌名称，避免提示文本因为空数据报错。
    /// </summary>
    private string GetCardName(Card card)
    {
        if (card == null || card.CardData == null)
        {
            return "未知卡牌";
        }

        return card.CardData.CardName;
    }

    /// <summary>
    /// 判断一张卡是否是法术牌。
    /// 法术牌点击后不会立刻结算，而是先进入选目标状态。
    /// </summary>
    private bool IsSpellCard(Card card)
    {
        return card != null &&
               card.CardData != null &&
               card.CardData.CardType == CardType.Spell;
    }

    /// <summary>
    /// 判断一张卡是否属于当前 UI 暂不支持的类型。
    /// 空卡或无效卡不在这里处理，会交给 Core 返回 InvalidCard。
    /// </summary>
    private bool IsUnsupportedCardType(Card card)
    {
        if (card == null || card.CardData == null) return false;

        return card.CardData.CardType != CardType.Minion &&
               card.CardData.CardType != CardType.Spell;
    }

    /// <summary>
    /// 从 Core 操作结果中读取反馈文本。
    /// 如果结果或文本为空，就根据成功/失败使用调用方给的默认文本。
    /// </summary>
    private string GetActionResultMessageOrFallback(
        GameActionResult result,
        string successFallback,
        string failureFallback)
    {
        if (result == null) return failureFallback ?? "";

        if (!string.IsNullOrWhiteSpace(result.Message))
        {
            return result.Message;
        }

        return result.Success ? successFallback ?? "" : failureFallback ?? "";
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
            if (string.IsNullOrWhiteSpace(feedbackMessage))
            {
                gameOverText.text = "";
                gameOverText.gameObject.SetActive(false);
                return;
            }

            gameOverText.gameObject.SetActive(true);
            gameOverText.text = feedbackMessage;
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
    /// 设置最近一次操作反馈。
    /// </summary>
    private void SetFeedback(string message)
    {
        feedbackMessage = message ?? "";
    }

    /// <summary>
    /// 清空操作反馈。
    /// </summary>
    private void ClearFeedback()
    {
        feedbackMessage = "";
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
