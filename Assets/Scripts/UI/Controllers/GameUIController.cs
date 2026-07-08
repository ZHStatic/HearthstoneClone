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
    [SerializeField] private BattlePresentationController presentationController;
    [SerializeField] private DeckSelectionController deckSelectionController;

    // 手牌和双方战场视图。
    [SerializeField] private HandView handView;
    [SerializeField] private BoardView playerBoardView;
    [SerializeField] private BoardView enemyBoardView;

    // HUD 文本：当前玩家、回合、法力、英雄血量、普通反馈、游戏结束提示。
    [SerializeField] private Text currentPlayerText;
    [SerializeField] private Text turnText;
    [SerializeField] private Text manaText;
    [SerializeField] private Text playerHeroText;
    [SerializeField] private Text enemyHeroText;
    [SerializeField] private Text feedbackText;
    [SerializeField] private Text gameOverText;

    // 英雄、英雄技能和结束回合按钮。
    // 英雄按钮用于让已选中的随从攻击英雄。
    [SerializeField] private Button playerHeroButton;
    [SerializeField] private Button enemyHeroButton;
    [SerializeField] private Button heroSkillButton;
    [SerializeField] private Button endTurnButton;

    // 按钮运行时颜色。位置、字号和美术样式仍然优先在 Unity Editor / Prefab 中调整。
    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color selectedButtonColor = new Color(1f, 0.9f, 0.35f, 1f);
    [SerializeField] private Color validTargetButtonColor = new Color(0.45f, 1f, 0.45f, 1f);
    [SerializeField] private Color invalidTargetButtonColor = new Color(1f, 0.55f, 0.55f, 1f);
    [SerializeField] private Color disabledButtonColor = new Color(0.65f, 0.65f, 0.65f, 1f);

    // 当前被选中的攻击者。
    // 第一次点击己方可攻击随从时设置，攻击或结束回合后清空。
    private Minion selectedAttacker;

    // 当前被选中的法术牌。
    // 点击法术牌时设置，点击目标、结束回合或改打其他牌后清空。
    private Card selectedSpellCard;

    // 当前是否正在等待玩家选择英雄技能目标。
    // 点击英雄技能按钮后设置，点击目标、结束回合或改做其他操作后清空。
    private bool isSelectingHeroSkillTarget;

    private string feedbackMessage = "";

    // Unity 生命周期方法：UI 创建后查找 GameManager、注册按钮事件并刷新一次界面。
    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (deckSelectionController == null)
        {
            deckSelectionController = FindObjectOfType<DeckSelectionController>();
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

        if (heroSkillButton != null)
        {
            heroSkillButton.onClick.AddListener(HandleHeroSkillClicked);
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

        if (heroSkillButton != null)
        {
            heroSkillButton.onClick.RemoveListener(HandleHeroSkillClicked);
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

        ClearOperationSelection();

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

        int logCountBefore = GetBattleLogCount();
        GameActionResult result = gameManager.TryPlayMinionCardDetailed(card);
        SetFeedback(GetActionResultMessageOrFallback(
            result,
            $"打出 {GetCardName(card)}。",
            "出牌失败。"));
        PlayPresentationForResult(result, logCountBefore);
        RefreshAll();
    }

    /// <summary>
    /// 选择一张法术牌，等待玩家点击目标。
    /// 出牌规则交给 GameManager 验证；UI 只记录“正在选择法术目标”的操作状态。
    /// </summary>
    private void SelectSpellCardForTargeting(Card card)
    {
        GameActionResult validationResult = gameManager.ValidatePlaySpellCard(card);
        if (validationResult.Failed)
        {
            SetFeedback(GetActionResultMessageOrFallback(
                validationResult,
                "",
                "现在不能使用这张法术牌。"));
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

        ClearOperationSelection();
        int logCountBefore = GetBattleLogCount();
        gameManager.EndTurn();
        SetFeedback($"{GetPlayerLabel(gameManager.CurrentPlayer)} 回合开始。");
        PlayPresentationForLatestBattleLog(logCountBefore, null);
        RefreshAll();
    }

    /// <summary>
    /// 处理英雄技能按钮点击。
    /// 点击后先检查当前玩家是否能使用技能，通过后进入选择目标状态。
    /// </summary>
    private void HandleHeroSkillClicked()
    {
        if (gameManager == null) return;

        if (isSelectingHeroSkillTarget)
        {
            ClearHeroSkillSelection();
            SetFeedback("取消选择英雄技能。");
            RefreshAll();
            return;
        }

        ClearOperationSelection();

        GameActionResult validationResult = gameManager.ValidateHeroSkill();
        if (validationResult.Failed)
        {
            SetFeedback(GetActionResultMessageOrFallback(
                validationResult,
                "",
                "现在不能使用英雄技能。"));
            RefreshAll();
            return;
        }

        isSelectingHeroSkillTarget = true;
        SetFeedback("已选择英雄技能，请选择敌方目标。");
        RefreshAll();
    }

    /// <summary>
    /// 处理玩家英雄被点击。
    /// 根据当前选择状态，把点击转换成法术、英雄技能或攻击目标。
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
        else if (isSelectingHeroSkillTarget)
        {
            TryUseSelectedHeroSkillOnHero(gameManager.Player.Hero);
        }
        else
        {
            TryAttackSelectedHero(gameManager.Player.Hero);
        }

        RefreshAll();
    }

    /// <summary>
    /// 处理敌方英雄被点击。
    /// 根据当前选择状态，把点击转换成法术、英雄技能或攻击目标。
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
        else if (isSelectingHeroSkillTarget)
        {
            TryUseSelectedHeroSkillOnHero(gameManager.Enemy.Hero);
        }
        else
        {
            TryAttackSelectedHero(gameManager.Enemy.Hero);
        }

        RefreshAll();
    }

    /// <summary>
    /// 处理随从点击。
    /// 根据当前选择状态，把点击转换成法术目标、英雄技能目标或攻击选择。
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
            return;
        }

        if (isSelectingHeroSkillTarget)
        {
            TryUseSelectedHeroSkillOnMinion(clickedMinion);
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
    }

    /// <summary>
    /// 尝试把某个随从设置为当前攻击者。
    /// 攻击规则交给 GameManager 验证；UI 只记录“正在选择攻击目标”的操作状态。
    /// </summary>
    private void SelectAttacker(Minion minion)
    {
        GameActionResult validationResult = gameManager.ValidateAttack(minion);
        if (validationResult.Failed)
        {
            ClearSelectedAttacker();
            SetFeedback(GetActionResultMessageOrFallback(
                validationResult,
                "",
                "现在不能选择这个随从攻击。"));
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

        string attackerName = GetMinionName(selectedAttacker);
        string targetName = GetMinionName(target);
        int logCountBefore = GetBattleLogCount();
        GameActionResult result = gameManager.TryAttackMinionDetailed(selectedAttacker, target);
        SetFeedback(GetActionResultMessageOrFallback(
            result,
            $"{attackerName} 攻击 {targetName}。",
            "目标非法，攻击失败。"));
        ClearSelectedAttacker();
        RefreshAll();
        PlayPresentationForResult(result, logCountBefore, GetMinionPulseTarget(target));
    }

    /// <summary>
    /// 使用当前选中的攻击者，尝试攻击目标英雄。
    /// 是否能攻击自己的英雄、是否是合法目标，仍然交给 GameManager 判断。
    /// </summary>
    private void TryAttackSelectedHero(Hero targetHero)
    {
        if (gameManager == null) return;

        if (selectedAttacker == null)
        {
            SetFeedback("请先选择一个可以攻击的随从。");
            return;
        }

        string attackerName = GetMinionName(selectedAttacker);
        string targetName = GetHeroName(targetHero);
        int logCountBefore = GetBattleLogCount();
        GameActionResult result = gameManager.TryAttackHeroDetailed(selectedAttacker, targetHero);
        SetFeedback(GetActionResultMessageOrFallback(
            result,
            $"{attackerName} 攻击 {targetName}。",
            "目标英雄非法，攻击失败。"));
        PlayPresentationForResult(result, logCountBefore, GetHeroPulseTarget(targetHero));
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

        string spellName = GetCardName(selectedSpellCard);
        string targetName = GetMinionName(target);

        int logCountBefore = GetBattleLogCount();
        GameActionResult result = gameManager.TryPlaySpellCardOnMinionDetailed(selectedSpellCard, target);
        string fallbackMessage = $"{spellName} 对 {targetName} 释放成功。";
        SetFeedback(GetActionResultMessageOrFallback(
            result,
            fallbackMessage,
            "法术目标非法，释放失败。"));
        ClearSelectedSpell();
        ClearSelectedAttacker();
        RefreshAll();
        PlayPresentationForResult(result, logCountBefore, GetMinionPulseTarget(target));
    }

    /// <summary>
    /// 使用当前选中的英雄技能，尝试对目标随从造成伤害。
    /// </summary>
    private void TryUseSelectedHeroSkillOnMinion(Minion target)
    {
        if (!isSelectingHeroSkillTarget)
        {
            SetFeedback("请先选择英雄技能。");
            return;
        }

        string targetName = GetMinionName(target);
        int logCountBefore = GetBattleLogCount();
        GameActionResult result = gameManager.TryUseHeroSkillOnMinionDetailed(target);
        SetFeedback(GetActionResultMessageOrFallback(
            result,
            $"英雄技能对 {targetName} 造成 {Player.HeroSkillDamage} 点伤害。",
            "英雄技能目标非法，使用失败。"));
        ClearOperationSelection();
        RefreshAll();
        PlayPresentationForResult(result, logCountBefore, GetMinionPulseTarget(target));
    }

    /// <summary>
    /// 使用当前选中的英雄技能，尝试对目标英雄造成伤害。
    /// </summary>
    private void TryUseSelectedHeroSkillOnHero(Hero targetHero)
    {
        if (!isSelectingHeroSkillTarget)
        {
            SetFeedback("请先选择英雄技能。");
            return;
        }

        string targetName = GetHeroName(targetHero);
        int logCountBefore = GetBattleLogCount();
        GameActionResult result = gameManager.TryUseHeroSkillOnHeroDetailed(targetHero);
        SetFeedback(GetActionResultMessageOrFallback(
            result,
            $"英雄技能对 {targetName} 造成 {Player.HeroSkillDamage} 点伤害。",
            "英雄技能目标英雄非法，使用失败。"));
        PlayPresentationForResult(result, logCountBefore, GetHeroPulseTarget(targetHero));
        ClearOperationSelection();
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

        string spellName = GetCardName(selectedSpellCard);
        string targetName = GetHeroName(targetHero);

        int logCountBefore = GetBattleLogCount();
        GameActionResult result = gameManager.TryPlaySpellCardOnHeroDetailed(selectedSpellCard, targetHero);
        string fallbackMessage = $"{spellName} 对 {targetName} 释放成功。";
        SetFeedback(GetActionResultMessageOrFallback(
            result,
            fallbackMessage,
            "法术目标英雄非法，释放失败。"));
        PlayPresentationForResult(result, logCountBefore, GetHeroPulseTarget(targetHero));
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
    /// 清空英雄技能选目标状态。
    /// </summary>
    private void ClearHeroSkillSelection()
    {
        isSelectingHeroSkillTarget = false;
    }

    /// <summary>
    /// 清空所有 UI 操作选择状态。
    /// 它只影响“下一次点击想做什么”，不修改 Core 层规则状态。
    /// </summary>
    private void ClearOperationSelection()
    {
        ClearSelectedAttacker();
        ClearSelectedSpell();
        ClearHeroSkillSelection();
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
            playerBoardView.Refresh(gameManager.Board.GetMinions(gameManager.Player), HandleMinionClicked, GetMinionHighlightState);
        }

        if (enemyBoardView != null)
        {
            enemyBoardView.Refresh(gameManager.Board.GetMinions(gameManager.Enemy), HandleMinionClicked, GetMinionHighlightState);
        }
    }

    /// <summary>
    /// 根据当前 UI 操作状态，计算一个随从应该显示什么高亮。
    /// 这里只读取 GameManager 的验证结果，不在 UI 中复制规则。
    /// </summary>
    private TargetHighlightState GetMinionHighlightState(Minion minion)
    {
        if (minion == null) return TargetHighlightState.None;

        if (selectedAttacker != null)
        {
            if (minion == selectedAttacker)
            {
                return TargetHighlightState.Selected;
            }

            return ToHighlightState(gameManager.ValidateAttackTarget(selectedAttacker, minion));
        }

        if (selectedSpellCard != null && selectedSpellCard.CardData != null)
        {
            return ToHighlightState(gameManager.ValidateSpellTargetMinion(selectedSpellCard.CardData, minion));
        }

        if (isSelectingHeroSkillTarget)
        {
            return ToHighlightState(gameManager.ValidateHeroSkillTargetMinion(minion));
        }

        return TargetHighlightState.None;
    }

    /// <summary>
    /// 根据当前 UI 操作状态，计算一个英雄按钮应该显示什么高亮。
    /// 英雄区域和随从一样只做提示，点击后仍然由 Core 做最终校验。
    /// </summary>
    private TargetHighlightState GetHeroHighlightState(Hero hero)
    {
        if (hero == null) return TargetHighlightState.None;

        if (selectedAttacker != null)
        {
            return ToHighlightState(gameManager.ValidateAttackHeroTarget(selectedAttacker, hero));
        }

        if (selectedSpellCard != null && selectedSpellCard.CardData != null)
        {
            return ToHighlightState(gameManager.ValidateSpellTargetHero(selectedSpellCard.CardData, hero));
        }

        if (isSelectingHeroSkillTarget)
        {
            return ToHighlightState(gameManager.ValidateHeroSkillTargetHero(hero));
        }

        return TargetHighlightState.None;
    }

    /// <summary>
    /// 把 Core 验证结果转换成 UI 高亮状态。
    /// </summary>
    private TargetHighlightState ToHighlightState(GameActionResult validationResult)
    {
        if (validationResult == null) return TargetHighlightState.Invalid;

        return validationResult.Success ? TargetHighlightState.Valid : TargetHighlightState.Invalid;
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
        RefreshFeedbackText();
        RefreshGameOverText();

        RefreshEndTurnButton();
        RefreshHeroButtons();
        RefreshHeroSkillButton();
    }

    /// <summary>
    /// 获取当前战斗日志数量，用于判断一次 UI 操作后有没有产生新的规则日志。
    /// </summary>
    private int GetBattleLogCount()
    {
        if (gameManager == null || gameManager.BattleLogger == null) return 0;

        return gameManager.BattleLogger.Count;
    }

    /// <summary>
    /// 如果 Core 操作成功，就播放这次操作产生的表现反馈。
    /// </summary>
    private void PlayPresentationForResult(GameActionResult result, int previousLogCount)
    {
        PlayPresentationForResult(result, previousLogCount, null);
    }

    /// <summary>
    /// 如果 Core 操作成功，就播放这次操作产生的表现反馈。
    /// preferredTarget 不为空时，表现层会优先让这个 UI 目标播放脉冲。
    /// </summary>
    private void PlayPresentationForResult(GameActionResult result, int previousLogCount, Transform preferredTarget)
    {
        if (result == null || result.Failed) return;

        PlayPresentationForLatestBattleLog(previousLogCount, result.LogEntry, preferredTarget);
    }

    /// <summary>
    /// 播放指定日志数量之后产生的最后一条日志。
    /// 如果没有新日志，就使用调用方传入的 fallbackEntry。
    /// </summary>
    private void PlayPresentationForLatestBattleLog(int previousLogCount, BattleLogEntry fallbackEntry = null)
    {
        PlayPresentationForLatestBattleLog(previousLogCount, fallbackEntry, null);
    }

    /// <summary>
    /// 播放指定日志数量之后产生的最后一条日志，并把指定 UI 目标作为优先脉冲目标。
    /// 如果本次操作已经结束游戏，优先让 GameOverText 播放反馈。
    /// </summary>
    private void PlayPresentationForLatestBattleLog(int previousLogCount, BattleLogEntry fallbackEntry, Transform preferredTarget)
    {
        if (presentationController == null) return;

        BattleLogEntry entry = fallbackEntry;
        if (gameManager != null && gameManager.BattleLogger != null && gameManager.BattleLogger.Count > previousLogCount)
        {
            entry = gameManager.BattleLogger.Entries[gameManager.BattleLogger.Count - 1];
        }

        presentationController.PlayLogFeedback(entry, GetPresentationPulseTarget(preferredTarget));
    }

    /// <summary>
    /// 根据英雄对象找到对应英雄按钮，作为受击或结算反馈的目标。
    /// 英雄按钮是稳定存在的 UI 物体，不会像随从 View 一样在 RefreshAll 时被重建。
    /// </summary>
    private Transform GetHeroPulseTarget(Hero hero)
    {
        if (hero == null || gameManager == null) return null;

        if (gameManager.Player != null && hero == gameManager.Player.Hero && playerHeroButton != null)
        {
            return playerHeroButton.transform;
        }

        if (gameManager.Enemy != null && hero == gameManager.Enemy.Hero && enemyHeroButton != null)
        {
            return enemyHeroButton.transform;
        }

        return null;
    }

    /// <summary>
    /// 根据随从对象找到它当前所在的 MinionView，作为受击反馈目标。
    /// 这个方法只在 RefreshAll 之后调用，确保 BoardView 的随从到 View 映射已经刷新。
    /// </summary>
    private Transform GetMinionPulseTarget(Minion minion)
    {
        if (minion == null || gameManager == null) return null;

        BoardView boardView = null;
        if (gameManager.Player != null && minion.Owner == gameManager.Player)
        {
            boardView = playerBoardView;
        }
        else if (gameManager.Enemy != null && minion.Owner == gameManager.Enemy)
        {
            boardView = enemyBoardView;
        }

        if (boardView == null) return null;

        return boardView.TryGetMinionView(minion, out MinionView minionView) && minionView != null
            ? minionView.transform
            : null;
    }

    /// <summary>
    /// 选择本次表现要脉冲的 UI 目标。
    /// 游戏结束时优先使用 GameOverText；否则使用调用方传入的具体目标。
    /// </summary>
    private Transform GetPresentationPulseTarget(Transform preferredTarget)
    {
        if (gameManager != null && gameManager.IsGameOver && gameOverText != null)
        {
            return gameOverText.transform;
        }

        return preferredTarget;
    }

    /// <summary>
    /// 当 GameManager 不存在时，清空所有 UI 显示。
    /// </summary>
    private void ClearAll()
    {
        ClearOperationSelection();
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
        SetText(feedbackText, "");
        SetText(gameOverText, "");

        ApplyButtonHighlightState(playerHeroButton, TargetHighlightState.None, false);
        ApplyButtonHighlightState(enemyHeroButton, TargetHighlightState.None, false);
        ApplyButtonHighlightState(heroSkillButton, TargetHighlightState.None, false);
        ApplyButtonHighlightState(endTurnButton, TargetHighlightState.None, false);
    }

    /// <summary>
    /// 生成当前玩家文本。
    /// 如果存在 UI 操作选择状态，会显示玩家当前正在选择什么。
    /// </summary>
    private string GetCurrentPlayerText(Player currentPlayer)
    {
        string text = $"Current: {GetPlayerLabel(currentPlayer)}";
        string operationText = GetCurrentOperationText();

        if (!string.IsNullOrWhiteSpace(operationText))
        {
            text += $" | {operationText}";
        }

        return text;
    }

    /// <summary>
    /// 生成当前 UI 操作状态文本，方便玩家知道下一次点击会被解释成什么操作。
    /// </summary>
    private string GetCurrentOperationText()
    {
        if (selectedAttacker != null && selectedAttacker.CardData != null)
        {
            return $"攻击：{selectedAttacker.CardData.CardName}";
        }

        if (selectedSpellCard != null && selectedSpellCard.CardData != null)
        {
            return $"法术：{selectedSpellCard.CardData.CardName}";
        }

        if (isSelectingHeroSkillTarget)
        {
            return "英雄技能";
        }

        return "";
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
    /// 安全获取英雄名称，避免提示文本因为空目标报错。
    /// </summary>
    private string GetHeroName(Hero hero)
    {
        if (hero == null)
        {
            return "未知英雄";
        }

        return hero.Name;
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
    /// 根据最近一次操作反馈刷新普通反馈文本。
    /// </summary>
    private void RefreshFeedbackText()
    {
        if (feedbackText == null) return;

        if (string.IsNullOrWhiteSpace(feedbackMessage))
        {
            feedbackText.text = "";
            feedbackText.gameObject.SetActive(false);
            return;
        }

        feedbackText.gameObject.SetActive(true);
        feedbackText.text = feedbackMessage;
    }

    /// <summary>
    /// 根据当前胜负状态显示或隐藏游戏结束文本。
    /// </summary>
    private void RefreshGameOverText()
    {
        if (!gameManager.IsGameOver)
        {
            if (gameOverText != null)
            {
                gameOverText.text = "";
                gameOverText.gameObject.SetActive(false);
            }

            return;
        }

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = gameManager.Winner == null
                ? "Game Over: Draw"
                : $"Game Over: {GetPlayerLabel(gameManager.Winner)} wins";
        }

        ShowSettlementIfGameOver();
    }

    private void ShowSettlementIfGameOver()
    {
        if (deckSelectionController == null) return;
        if (gameManager == null || !gameManager.IsGameOver) return;

        deckSelectionController.ShowSettlement();
    }

    /// <summary>
    /// 刷新结束回合按钮状态。
    /// </summary>
    private void RefreshEndTurnButton()
    {
        ApplyButtonHighlightState(endTurnButton, TargetHighlightState.None, !gameManager.IsGameOver);
    }

    /// <summary>
    /// 刷新双方英雄按钮的目标高亮。
    /// </summary>
    private void RefreshHeroButtons()
    {
        Hero playerHero = gameManager.Player?.Hero;
        Hero enemyHero = gameManager.Enemy?.Hero;

        ApplyButtonHighlightState(playerHeroButton, GetHeroHighlightState(playerHero), !gameManager.IsGameOver);
        ApplyButtonHighlightState(enemyHeroButton, GetHeroHighlightState(enemyHero), !gameManager.IsGameOver);
    }

    /// <summary>
    /// 刷新英雄技能按钮状态。
    /// 游戏未结束时保持可点，让费用不足或本回合已使用也能显示 Core 返回的失败原因。
    /// </summary>
    private void RefreshHeroSkillButton()
    {
        if (gameManager.IsGameOver)
        {
            ApplyButtonHighlightState(heroSkillButton, TargetHighlightState.None, false);
            return;
        }

        TargetHighlightState highlightState = isSelectingHeroSkillTarget
            ? TargetHighlightState.Selected
            : ToHighlightState(gameManager.ValidateHeroSkill());

        ApplyButtonHighlightState(heroSkillButton, highlightState, true);
    }

    /// <summary>
    /// 把目标高亮状态套用到按钮上。
    /// </summary>
    private void ApplyButtonHighlightState(Button button, TargetHighlightState highlightState, bool interactable)
    {
        if (button == null) return;

        button.interactable = interactable;

        Graphic targetGraphic = button.targetGraphic;
        if (targetGraphic == null) return;

        if (!interactable)
        {
            targetGraphic.color = disabledButtonColor;
            return;
        }

        targetGraphic.color = highlightState switch
        {
            TargetHighlightState.Selected => selectedButtonColor,
            TargetHighlightState.Valid => validTargetButtonColor,
            TargetHighlightState.Invalid => invalidTargetButtonColor,
            _ => normalButtonColor,
        };
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
