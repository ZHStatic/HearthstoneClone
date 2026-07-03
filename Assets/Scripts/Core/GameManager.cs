using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对局管理器。
/// 负责一局游戏的初始化、回合切换、出牌、攻击、死亡清理和胜负判断。
/// 之后 UI 可以调用它的方法，但它本身不直接依赖 UI。
/// </summary>
public partial class GameManager : MonoBehaviour
{
    // 在 Inspector 里配置的双方初始牌库模板。
    // 每个元素都是一个 CardData asset，运行时会被 Player 转成 Card 实例。
    [SerializeField] private List<CardData> playerDeckData = new List<CardData>();
    [SerializeField] private List<CardData> enemyDeckData = new List<CardData>();

    // 开局每名玩家抽几张牌。先用 3，之后如果要还原炉石规则可以再扩展。
    [SerializeField] private int startingHandCount = 3;
    // 调试开关：回合开始后打印当前玩家的合法动作列表。
    // 只用于验证 GameActionGenerator，不会执行任何动作。
    [SerializeField] private bool logLegalActionsOnTurnStart = false;
    // 调试开关：关闭洗牌后，Inspector 中的牌库顺序会更稳定，方便验证 AI 起手和出牌选择。
    [SerializeField] private bool disableDeckShuffleForDebug = false;
    // 运行时对象：进入 Play 模式后由 StartNewGame 创建。
    public Player Player { get; private set; }
    public Player Enemy { get; private set; }
    public Board Board { get; private set; }
    public GameEventBus EventBus { get; private set; }
    public BattleLogger BattleLogger { get; private set; }
    public BattleLogEntry LastActionLogEntry { get; private set; }

    // 当前正在行动的玩家，以及本局游戏的结果状态。
    public Player CurrentPlayer { get; private set; }
    public Player Winner { get; private set; }
    public int TurnNumber { get; private set; }
    public bool IsGameOver { get; private set; }

    // Unity 生命周期方法：Awake 会早于其他脚本的 Start 执行。
    // 这样 UI 脚本在 Start 里刷新时，Player / Enemy / Board 已经创建好了。
    private void Awake()
    {
        StartNewGame();
    }

    /// <summary>
    /// 开始一局新游戏：创建玩家、战场、抽起手牌，然后进入玩家的第一个回合。
    /// </summary>
    public void StartNewGame()
    {
        EventBus = new GameEventBus();
        BattleLogger = new BattleLogger();
        LastActionLogEntry = null;

        bool shuffleDeck = !disableDeckShuffleForDebug;
        Player = new Player(playerDeckData, "Player", shuffleDeck: shuffleDeck);
        Enemy = new Player(enemyDeckData, "Enemy", shuffleDeck: shuffleDeck);
        Board = new Board(Player, Enemy);
        InitializeEnemyAI();

        CurrentPlayer = Player;
        Winner = null;
        TurnNumber = 0;
        IsGameOver = false;

        SubscribeGameplayEventHandlers();
        DrawStartingHands(Player);
        DrawStartingHands(Enemy);
        StartTurn(CurrentPlayer);
    }

    /// <summary>
    /// 给指定玩家抽起手牌。
    /// </summary>
    public void DrawStartingHands(Player targetPlayer)
    {
        if (targetPlayer == null) return;

        int drawCount = startingHandCount > 0 ? startingHandCount : 0;
        for (int i = 0; i < drawCount; i++)
        {
            targetPlayer.DrawCard();
        }
    }

    /// <summary>
    /// 开始指定玩家的回合。
    /// Player.StartTurn 会处理法力水晶、重置手牌费用、抽一张牌。
    /// GameManager 额外负责让该玩家场上的随从恢复攻击权限。
    /// </summary>
    public void StartTurn(Player targetPlayer)
    {
        if (targetPlayer == null) return;
        if (IsGameOver) return;

        CurrentPlayer = targetPlayer;
        TurnNumber++;

        targetPlayer.StartTurn();

        RecordBattleLog(
            BattleLogEntryType.TurnStarted,
            sourcePlayer: targetPlayer,
            message: $"{GetPlayerLogName(targetPlayer)} 回合开始。");

        IReadOnlyList<Minion> minions = Board.GetMinions(targetPlayer);
        if (minions != null)
        {
            foreach (Minion minion in minions)
            {
                minion.SetCanAttack(true);
            }
        }

        LogLegalActionsForCurrentPlayer();
        // AI 自动行动和快照调试钩子实现在 Assets/Scripts/AI/GameManager.AIBridge.cs。
        LogSnapshotEvaluationForCurrentPlayer();
        LogSnapshotSimulationForCurrentPlayer();
        LogAIHandForCurrentPlayer();
        TryRunEnemyAI();
    }

    /// <summary>
    /// 结束当前玩家的回合，并切换到对手回合。
    /// </summary>
    public void EndTurn()
    {
        if (IsGameOver) return;

        RecordBattleLog(
            BattleLogEntryType.TurnEnded,
            sourcePlayer: CurrentPlayer,
            message: $"{GetPlayerLogName(CurrentPlayer)} 回合结束。");

        Player nextPlayer = GetOpponent(CurrentPlayer);
        StartTurn(nextPlayer);
    }

    /// <summary>
    /// 根据一名玩家，返回他的对手。
    /// 如果传入的不是本局里的 Player 或 Enemy，则返回 null。
    /// </summary>
    public Player GetOpponent(Player targetPlayer)
    {
        if (targetPlayer == Player) return Enemy;
        if (targetPlayer == Enemy) return Player;

        return null;
    }

    /// <summary>
    /// 执行一条动作。
    /// 玩家输入和阶段 3 AI 都可以走这个入口，避免各自调用不同规则方法。
    /// </summary>
    public GameActionResult ExecuteAction(GameAction action)
    {
        if (action == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.Unknown,
                "动作无效。");
        }

        GameActionResult actorValidationResult = ValidateActionActor(action.Actor);
        if (actorValidationResult.Failed) return actorValidationResult;

        switch (action.ActionType)
        {
            case GameActionType.PlayMinionCard:
                return TryPlayMinionCardDetailed(action.Card);
            case GameActionType.PlaySpellOnMinion:
                return TryPlaySpellCardOnMinionDetailed(action.Card, action.TargetMinion);
            case GameActionType.PlaySpellOnHero:
                return TryPlaySpellCardOnHeroDetailed(action.Card, action.TargetHero);
            case GameActionType.AttackMinion:
                return TryAttackMinionDetailed(action.Attacker, action.TargetMinion);
            case GameActionType.AttackHero:
                return TryAttackHeroDetailed(action.Attacker, action.TargetHero);
            case GameActionType.UseHeroSkillOnMinion:
                return TryUseHeroSkillOnMinionDetailed(action.TargetMinion);
            case GameActionType.UseHeroSkillOnHero:
                return TryUseHeroSkillOnHeroDetailed(action.TargetHero);
            case GameActionType.EndTurn:
                EndTurn();
                return GameActionResult.Succeeded($"{GetPlayerLogName(CurrentPlayer)} 回合开始。");
            default:
                return GameActionResult.FailedWith(
                    GameActionFailureReason.Unknown,
                    "暂不支持这个动作。");
        }
    }

    /// <summary>
    /// 检查动作发起者是否是当前行动玩家。
    /// </summary>
    private GameActionResult ValidateActionActor(Player actor)
    {
        if (IsGameOver)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.GameOver,
                "游戏已经结束，不能继续执行动作。");
        }

        if (CurrentPlayer == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.NoCurrentPlayer,
                "当前没有行动玩家。");
        }

        if (actor != CurrentPlayer)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.NotCurrentPlayer,
                "只能执行当前玩家的动作。");
        }

        return GameActionResult.Succeeded();
    }

    /// <summary>
    /// 调试用：打印当前玩家在这个局面下可以执行的合法动作。
    /// 这个方法只读取 GameActionGenerator 的结果，不执行任何动作。
    /// </summary>
    private void LogLegalActionsForCurrentPlayer()
    {
        if (!logLegalActionsOnTurnStart) return;

        List<GameAction> legalActions = GameActionGenerator.GenerateLegalActions(this);
        Debug.Log($"Legal Actions - {GetPlayerLogName(CurrentPlayer)}: {legalActions.Count}");

        for (int i = 0; i < legalActions.Count; i++)
        {
            Debug.Log($"{i + 1}. {GetActionDebugText(legalActions[i])}");
        }
    }

    /// <summary>
    /// 把一条动作转换成 Console 中容易阅读的文本。
    /// 只用于调试动作枚举结果，不参与正式规则结算。
    /// </summary>
    private string GetActionDebugText(GameAction action)
    {
        if (action == null) return "None";

        return action.ActionType switch
        {
            GameActionType.PlayMinionCard => $"PlayMinionCard: {GetCardLogName(action.Card)}",
            GameActionType.PlaySpellOnMinion => $"PlaySpellOnMinion: {GetCardLogName(action.Card)} -> {GetMinionLogName(action.TargetMinion)}",
            GameActionType.PlaySpellOnHero => $"PlaySpellOnHero: {GetCardLogName(action.Card)} -> {GetHeroLogName(action.TargetHero)}",
            GameActionType.AttackMinion => $"AttackMinion: {GetMinionLogName(action.Attacker)} -> {GetMinionLogName(action.TargetMinion)}",
            GameActionType.AttackHero => $"AttackHero: {GetMinionLogName(action.Attacker)} -> {GetHeroLogName(action.TargetHero)}",
            GameActionType.UseHeroSkillOnMinion => $"UseHeroSkillOnMinion: {GetPlayerLogName(action.Actor)} -> {GetMinionLogName(action.TargetMinion)}",
            GameActionType.UseHeroSkillOnHero => $"UseHeroSkillOnHero: {GetPlayerLogName(action.Actor)} -> {GetHeroLogName(action.TargetHero)}",
            GameActionType.EndTurn => "EndTurn",
            _ => action.ActionType.ToString(),
        };
    }

    /// <summary>
    /// 尝试把当前玩家手牌中的一张随从牌召唤到战场。
    /// 这是兼容旧调用方的 bool 入口；新流程优先使用 TryPlayMinionCardDetailed 或 ExecuteAction。
    /// </summary>
    public bool TryPlayMinionCard(Card card)
    {
        return TryPlayMinionCardDetailed(card).Success;
    }

    /// <summary>
    /// 尝试打出随从牌，并返回更详细的操作结果。
    /// UI 和 AI 应优先使用这个入口或 ExecuteAction，以便读取失败原因和反馈文本。
    /// </summary>
    public GameActionResult TryPlayMinionCardDetailed(Card card)
    {
        GameActionResult validationResult = ValidatePlayMinionCard(card);
        if (validationResult.Failed) return validationResult;

        bool played = CurrentPlayer.PlayCard(card);
        if (!played)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.Unknown,
                "出牌失败。");
        }

        Minion minion = new Minion(card.CardData, CurrentPlayer);
        bool summoned = Board.SummonMinion(minion);
        if (!summoned)
        {
            return GameActionResult.FailedWith(
                Board.CanSummon(CurrentPlayer) ? GameActionFailureReason.Unknown : GameActionFailureReason.BoardFull,
                "召唤随从失败。");
        }

        string cardName = GetCardLogName(card);
        RecordCardPlayed(card, CurrentPlayer);
        RecordMinionSummoned(minion);
        BattleLogEntry lastActionBeforeSummonEffects = LastActionLogEntry;
        ResolveAfterSummon(minion);
        BattleLogEntry summonEffectLogEntry = LastActionLogEntry != lastActionBeforeSummonEffects
            ? LastActionLogEntry
            : null;

        return BuildPlayMinionSuccessResult(summonEffectLogEntry, $"打出 {cardName}。");
    }

    /// <summary>
    /// 检查当前玩家能否打出这张随从牌。
    /// 这里只做条件判断，不扣法力、不移除手牌、不创建随从。
    /// </summary>
    internal GameActionResult ValidatePlayMinionCard(Card card)
    {
        if (card == null || card.CardData == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidCard,
                "这张卡的数据无效。");
        }

        if (IsGameOver)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.GameOver,
                "游戏已经结束，不能继续出牌。");
        }

        if (CurrentPlayer == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.NoCurrentPlayer,
                "当前没有行动玩家。");
        }

        if (!CurrentPlayer.HasCardInHand(card))
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.CardNotInHand,
                "这张卡不在当前玩家手牌里。");
        }

        if (card.CurrentCost > CurrentPlayer.CurrentMana)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.NotEnoughMana,
                $"{GetCardLogName(card)} 需要 {card.CurrentCost} 点法力，当前只有 {CurrentPlayer.CurrentMana} 点。");
        }

        if (card.CardData.CardType != CardType.Minion)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.WrongCardType,
                "当前操作只能打出随从牌。");
        }

        if (Board == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.BoardUnavailable,
                "战场还没有准备好。");
        }

        if (!Board.CanSummon(CurrentPlayer))
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.BoardFull,
                "战场已满，不能召唤更多随从。");
        }

        return GameActionResult.Succeeded();
    }

    /// <summary>
    /// 尝试把当前玩家手牌中的单目标伤害法术打到一个随从身上。
    /// 这是兼容旧调用方的 bool 入口；新流程优先使用 TryPlaySpellCardOnMinionDetailed 或 ExecuteAction。
    /// </summary>
    public bool TryPlaySpellCardOnMinion(Card card, Minion target)
    {
        return TryPlaySpellCardOnMinionDetailed(card, target).Success;
    }

    /// <summary>
    /// 尝试把当前玩家手牌中的单目标伤害法术打到一个随从身上，并返回详细操作结果。
    /// </summary>
    public GameActionResult TryPlaySpellCardOnMinionDetailed(Card card, Minion target)
    {
        GameActionResult cardValidationResult = ValidatePlaySpellCard(card);
        if (cardValidationResult.Failed) return cardValidationResult;

        GameActionResult targetValidationResult = ValidateSpellTargetMinion(card.CardData, target);
        if (targetValidationResult.Failed) return targetValidationResult;

        bool played = CurrentPlayer.PlayCard(card);
        if (!played)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.Unknown,
                "法术释放失败。");
        }

        RecordCardPlayed(card, CurrentPlayer);
        BattleLogEntry spellLogEntry = DamageMinion(target, card.CardData.SpellDamage, GetCardLogName(card), CurrentPlayer, BattleLogEntryType.Spell);

        CleanupDeadMinions();
        CheckGameOver();
        return BuildSpellSuccessResult(spellLogEntry, $"{GetCardLogName(card)} 对 {GetMinionLogName(target)} 释放成功。");
    }

    /// <summary>
    /// 尝试把当前玩家手牌中的单目标伤害法术打到一个英雄身上。
    /// 这是兼容旧调用方的 bool 入口；新流程优先使用 TryPlaySpellCardOnHeroDetailed 或 ExecuteAction。
    /// </summary>
    public bool TryPlaySpellCardOnHero(Card card, Hero targetHero)
    {
        return TryPlaySpellCardOnHeroDetailed(card, targetHero).Success;
    }

    /// <summary>
    /// 尝试把当前玩家手牌中的单目标伤害法术打到一个英雄身上，并返回详细操作结果。
    /// </summary>
    public GameActionResult TryPlaySpellCardOnHeroDetailed(Card card, Hero targetHero)
    {
        GameActionResult cardValidationResult = ValidatePlaySpellCard(card);
        if (cardValidationResult.Failed) return cardValidationResult;

        GameActionResult targetValidationResult = ValidateSpellTargetHero(card.CardData, targetHero);
        if (targetValidationResult.Failed) return targetValidationResult;

        bool played = CurrentPlayer.PlayCard(card);
        if (!played)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.Unknown,
                "法术释放失败。");
        }

        RecordCardPlayed(card, CurrentPlayer);
        BattleLogEntry spellLogEntry = DamageHero(targetHero, card.CardData.SpellDamage, GetCardLogName(card), CurrentPlayer, BattleLogEntryType.Spell);

        CleanupDeadMinions();
        CheckGameOver();
        return BuildSpellSuccessResult(spellLogEntry, $"{GetCardLogName(card)} 对 {GetHeroLogName(targetHero)} 释放成功。");
    }

    /// <summary>
    /// 检查当前玩家能否打出这张法术牌。
    /// 这里只检查卡牌本身，不检查具体目标。
    /// </summary>
    internal GameActionResult ValidatePlaySpellCard(Card card)
    {
        if (card == null || card.CardData == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidCard,
                "这张卡的数据无效。");
        }

        if (IsGameOver)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.GameOver,
                "游戏已经结束，不能继续施放法术。");
        }

        if (CurrentPlayer == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.NoCurrentPlayer,
                "当前没有行动玩家。");
        }

        if (!CurrentPlayer.HasCardInHand(card))
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.CardNotInHand,
                "这张卡不在当前玩家手牌里。");
        }

        if (card.CurrentCost > CurrentPlayer.CurrentMana)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.NotEnoughMana,
                $"{GetCardLogName(card)} 需要 {card.CurrentCost} 点法力，当前只有 {CurrentPlayer.CurrentMana} 点。");
        }

        if (card.CardData.CardType != CardType.Spell)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.WrongCardType,
                "当前操作只能打出法术牌。");
        }

        return GameActionResult.Succeeded();
    }

    /// <summary>
    /// 检查一个随从是否能成为当前法术的目标。
    /// </summary>
    internal GameActionResult ValidateSpellTargetMinion(CardData spellData, Minion target)
    {
        if (spellData == null || spellData.CardType != CardType.Spell)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.WrongCardType,
                "当前操作只能打出法术牌。");
        }

        if (target == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidTarget,
                "法术目标无效。");
        }

        if (target.IsDead)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidTarget,
                $"{GetMinionLogName(target)} 已经死亡，不能成为法术目标。");
        }

        Player targetOwner = target.Owner;
        if (targetOwner != Player && targetOwner != Enemy)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidTarget,
                "法术目标不属于本局。");
        }

        Player opponent = GetOpponent(CurrentPlayer);
        switch (spellData.SpellTargetType)
        {
            case SpellTargetType.AnyCharacter:
            case SpellTargetType.Minion:
                return GameActionResult.Succeeded();
            case SpellTargetType.EnemyCharacter:
            case SpellTargetType.EnemyMinion:
                if (targetOwner == opponent) return GameActionResult.Succeeded();

                return GameActionResult.FailedWith(
                    targetOwner == CurrentPlayer ? GameActionFailureReason.TargetIsFriendly : GameActionFailureReason.InvalidTarget,
                    "这个法术不能选择友方随从。");
            case SpellTargetType.FriendlyCharacter:
            case SpellTargetType.FriendlyMinion:
                if (targetOwner == CurrentPlayer) return GameActionResult.Succeeded();

                return GameActionResult.FailedWith(
                    GameActionFailureReason.InvalidTarget,
                    "这个法术只能选择友方随从。");
            default:
                return GameActionResult.FailedWith(
                    GameActionFailureReason.InvalidTarget,
                    "这个法术不能选择随从作为目标。");
        }
    }

    /// <summary>
    /// 检查一个英雄是否能成为当前法术的目标。
    /// </summary>
    internal GameActionResult ValidateSpellTargetHero(CardData spellData, Hero targetHero)
    {
        if (spellData == null || spellData.CardType != CardType.Spell)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.WrongCardType,
                "当前操作只能打出法术牌。");
        }

        if (targetHero == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidTarget,
                "法术目标英雄无效。");
        }

        if (targetHero.IsDead)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidTarget,
                $"{GetHeroLogName(targetHero)} 已经死亡，不能成为法术目标。");
        }

        Player targetOwner = null;
        if (Player != null && targetHero == Player.Hero)
        {
            targetOwner = Player;
        }
        else if (Enemy != null && targetHero == Enemy.Hero)
        {
            targetOwner = Enemy;
        }

        if (targetOwner == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidTarget,
                "法术目标英雄不属于本局。");
        }

        Player opponent = GetOpponent(CurrentPlayer);
        switch (spellData.SpellTargetType)
        {
            case SpellTargetType.AnyCharacter:
                return GameActionResult.Succeeded();
            case SpellTargetType.EnemyCharacter:
                if (targetOwner == opponent) return GameActionResult.Succeeded();

                return GameActionResult.FailedWith(
                    targetOwner == CurrentPlayer ? GameActionFailureReason.TargetIsFriendly : GameActionFailureReason.InvalidTarget,
                    "这个法术不能选择友方英雄。");
            case SpellTargetType.FriendlyCharacter:
                if (targetOwner == CurrentPlayer) return GameActionResult.Succeeded();

                return GameActionResult.FailedWith(
                    GameActionFailureReason.InvalidTarget,
                    "这个法术只能选择友方英雄。");
            default:
                return GameActionResult.FailedWith(
                    GameActionFailureReason.InvalidTarget,
                    "这个法术不能选择英雄作为目标。");
        }
    }

    /// <summary>
    /// 使用出随从后的结算日志创建成功结果。
    /// 如果战吼等召唤后效果造成伤害或结束游戏，优先返回实际结算日志。
    /// </summary>
    private GameActionResult BuildPlayMinionSuccessResult(BattleLogEntry summonEffectLogEntry, string fallbackMessage)
    {
        BattleLogEntry resultLogEntry = IsGameOver && LastActionLogEntry != null
            ? LastActionLogEntry
            : summonEffectLogEntry;

        string message = resultLogEntry != null && !string.IsNullOrWhiteSpace(resultLogEntry.Message)
            ? resultLogEntry.Message
            : fallbackMessage ?? "";

        return GameActionResult.Succeeded(message, resultLogEntry);
    }

    /// <summary>
    /// 使用法术结算日志创建成功结果。
    /// 如果法术导致游戏结束，优先返回游戏结束日志。
    /// 如果没有日志文本，就使用传入的默认成功反馈。
    /// </summary>
    private GameActionResult BuildSpellSuccessResult(BattleLogEntry spellLogEntry, string fallbackMessage)
    {
        BattleLogEntry resultLogEntry = IsGameOver && LastActionLogEntry != null
            ? LastActionLogEntry
            : spellLogEntry;

        string message = resultLogEntry != null && !string.IsNullOrWhiteSpace(resultLogEntry.Message)
            ? resultLogEntry.Message
            : fallbackMessage ?? "";

        return GameActionResult.Succeeded(message, resultLogEntry);
    }

    /// <summary>
    /// 使用攻击结算日志创建成功结果。
    /// 如果攻击导致游戏结束，优先返回游戏结束日志。
    /// </summary>
    private GameActionResult BuildAttackSuccessResult(BattleLogEntry attackLogEntry, string fallbackMessage)
    {
        BattleLogEntry resultLogEntry = IsGameOver && LastActionLogEntry != null
            ? LastActionLogEntry
            : attackLogEntry;

        string message = resultLogEntry != null && !string.IsNullOrWhiteSpace(resultLogEntry.Message)
            ? resultLogEntry.Message
            : fallbackMessage ?? "";

        return GameActionResult.Succeeded(message, resultLogEntry);
    }

    /// <summary>
    /// 尝试对敌方随从使用英雄技能。
    /// 第一版英雄技能固定为 2 费、每回合一次、造成 1 点伤害。
    /// </summary>
    public GameActionResult TryUseHeroSkillOnMinionDetailed(Minion target)
    {
        GameActionResult skillValidationResult = ValidateHeroSkill();
        if (skillValidationResult.Failed) return skillValidationResult;

        GameActionResult targetValidationResult = ValidateHeroSkillTargetMinion(CurrentPlayer, target);
        if (targetValidationResult.Failed) return targetValidationResult;

        if (!CurrentPlayer.SpendMana(Player.HeroSkillCost))
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.NotEnoughMana,
                $"英雄技能需要 {Player.HeroSkillCost} 点法力，当前只有 {CurrentPlayer.CurrentMana} 点。");
        }

        if (!CurrentPlayer.MarkHeroSkillUsed())
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.HeroSkillAlreadyUsed,
                "本回合已经使用过英雄技能。");
        }

        BattleLogEntry heroSkillLogEntry = DamageMinion(
            target,
            Player.HeroSkillDamage,
            "英雄技能",
            CurrentPlayer,
            BattleLogEntryType.HeroSkill);

        CleanupDeadMinions();
        CheckGameOver();
        return BuildHeroSkillSuccessResult(heroSkillLogEntry, $"英雄技能对 {GetMinionLogName(target)} 造成 {Player.HeroSkillDamage} 点伤害。");
    }

    /// <summary>
    /// 尝试对敌方英雄使用英雄技能。
    /// </summary>
    public GameActionResult TryUseHeroSkillOnHeroDetailed(Hero targetHero)
    {
        GameActionResult skillValidationResult = ValidateHeroSkill();
        if (skillValidationResult.Failed) return skillValidationResult;

        GameActionResult targetValidationResult = ValidateHeroSkillTargetHero(CurrentPlayer, targetHero);
        if (targetValidationResult.Failed) return targetValidationResult;

        if (!CurrentPlayer.SpendMana(Player.HeroSkillCost))
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.NotEnoughMana,
                $"英雄技能需要 {Player.HeroSkillCost} 点法力，当前只有 {CurrentPlayer.CurrentMana} 点。");
        }

        if (!CurrentPlayer.MarkHeroSkillUsed())
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.HeroSkillAlreadyUsed,
                "本回合已经使用过英雄技能。");
        }

        BattleLogEntry heroSkillLogEntry = DamageHero(
            targetHero,
            Player.HeroSkillDamage,
            "英雄技能",
            CurrentPlayer,
            BattleLogEntryType.HeroSkill);

        CheckGameOver();
        return BuildHeroSkillSuccessResult(heroSkillLogEntry, $"英雄技能对 {GetHeroLogName(targetHero)} 造成 {Player.HeroSkillDamage} 点伤害。");
    }

    /// <summary>
    /// 检查当前玩家是否可以使用英雄技能。
    /// 这里只检查使用者状态，不检查具体目标。
    /// </summary>
    internal GameActionResult ValidateHeroSkill()
    {
        if (IsGameOver)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.GameOver,
                "游戏已经结束，不能继续使用英雄技能。");
        }

        if (CurrentPlayer == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.NoCurrentPlayer,
                "当前没有行动玩家。");
        }

        if (CurrentPlayer.HasUsedHeroSkillThisTurn)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.HeroSkillAlreadyUsed,
                "本回合已经使用过英雄技能。");
        }

        if (!CurrentPlayer.CanSpendMana(Player.HeroSkillCost))
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.NotEnoughMana,
                $"英雄技能需要 {Player.HeroSkillCost} 点法力，当前只有 {CurrentPlayer.CurrentMana} 点。");
        }

        return GameActionResult.Succeeded();
    }

    /// <summary>
    /// 检查一个随从是否能成为当前英雄技能目标。
    /// 第一版英雄技能只能选择敌方随从，不受嘲讽限制。
    /// </summary>
    internal GameActionResult ValidateHeroSkillTargetMinion(Minion target)
    {
        return ValidateHeroSkillTargetMinion(CurrentPlayer, target);
    }

    /// <summary>
    /// 检查一个随从是否能成为指定玩家的英雄技能目标。
    /// 第一版英雄技能只能选择敌方随从，不受嘲讽限制。
    /// </summary>
    internal GameActionResult ValidateHeroSkillTargetMinion(Player actor, Minion target)
    {
        if (target == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidTarget,
                "英雄技能目标无效。");
        }

        if (target.IsDead)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidTarget,
                $"{GetMinionLogName(target)} 已经死亡，不能成为英雄技能目标。");
        }

        Player opponent = GetOpponent(actor);
        if (opponent == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidTarget,
                "找不到当前玩家的对手。");
        }

        if (target.Owner != opponent)
        {
            return GameActionResult.FailedWith(
                target.Owner == actor ? GameActionFailureReason.TargetIsFriendly : GameActionFailureReason.InvalidTarget,
                "英雄技能只能选择敌方随从。");
        }

        return GameActionResult.Succeeded();
    }

    /// <summary>
    /// 检查一个英雄是否能成为当前英雄技能目标。
    /// 第一版英雄技能只能选择敌方英雄。
    /// </summary>
    internal GameActionResult ValidateHeroSkillTargetHero(Hero targetHero)
    {
        return ValidateHeroSkillTargetHero(CurrentPlayer, targetHero);
    }

    /// <summary>
    /// 检查一个英雄是否能成为指定玩家的英雄技能目标。
    /// 第一版英雄技能只能选择敌方英雄。
    /// </summary>
    internal GameActionResult ValidateHeroSkillTargetHero(Player actor, Hero targetHero)
    {
        if (targetHero == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidTarget,
                "英雄技能目标英雄无效。");
        }

        if (targetHero.IsDead)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidTarget,
                $"{GetHeroLogName(targetHero)} 已经死亡，不能成为英雄技能目标。");
        }

        Player opponent = GetOpponent(actor);
        if (opponent == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidTarget,
                "找不到当前玩家的对手。");
        }

        if (targetHero != opponent.Hero)
        {
            return GameActionResult.FailedWith(
                actor != null && targetHero == actor.Hero ? GameActionFailureReason.TargetIsFriendly : GameActionFailureReason.InvalidTarget,
                "英雄技能只能选择敌方英雄。");
        }

        return GameActionResult.Succeeded();
    }

    /// <summary>
    /// 使用英雄技能结算日志创建成功结果。
    /// 如果英雄技能导致游戏结束，优先返回游戏结束日志。
    /// </summary>
    private GameActionResult BuildHeroSkillSuccessResult(BattleLogEntry heroSkillLogEntry, string fallbackMessage)
    {
        BattleLogEntry resultLogEntry = IsGameOver && LastActionLogEntry != null
            ? LastActionLogEntry
            : heroSkillLogEntry;

        string message = resultLogEntry != null && !string.IsNullOrWhiteSpace(resultLogEntry.Message)
            ? resultLogEntry.Message
            : fallbackMessage ?? "";

        return GameActionResult.Succeeded(message, resultLogEntry);
    }

    /// <summary>
    /// 尝试让一个随从攻击另一个随从。
    /// 这是兼容旧调用方的 bool 入口；新流程优先使用 TryAttackMinionDetailed 或 ExecuteAction。
    /// </summary>
    public bool TryAttackMinion(Minion attacker, Minion target)
    {
        return TryAttackMinionDetailed(attacker, target).Success;
    }

    /// <summary>
    /// 尝试让一个随从攻击另一个随从，并返回详细操作结果。
    /// </summary>
    public GameActionResult TryAttackMinionDetailed(Minion attacker, Minion target)
    {
        GameActionResult validationResult = ValidateAttackTarget(attacker, target);
        if (validationResult.Failed) return validationResult;

        RecordAttack(attacker, GetMinionLogName(target));
        DamageMinion(attacker, target.Attack, GetMinionLogName(target), target.Owner, BattleLogEntryType.Damage);
        BattleLogEntry targetDamageLogEntry = DamageMinion(target, attacker.Attack, GetMinionLogName(attacker), attacker.Owner, BattleLogEntryType.Damage);
        attacker.SetCanAttack(false);

        CleanupDeadMinions();
        CheckGameOver();
        return BuildAttackSuccessResult(targetDamageLogEntry, $"{GetMinionLogName(attacker)} 攻击 {GetMinionLogName(target)}。");
    }

    /// <summary>
    /// 尝试让一个随从攻击对方英雄。
    /// 这是兼容旧调用方的 bool 入口；新流程优先使用 TryAttackHeroDetailed 或 ExecuteAction。
    /// </summary>
    public bool TryAttackHero(Minion attacker, Hero targetHero)
    {
        return TryAttackHeroDetailed(attacker, targetHero).Success;
    }

    /// <summary>
    /// 尝试让一个随从攻击对方英雄，并返回详细操作结果。
    /// </summary>
    public GameActionResult TryAttackHeroDetailed(Minion attacker, Hero targetHero)
    {
        GameActionResult validationResult = ValidateAttackHeroTarget(attacker, targetHero);
        if (validationResult.Failed) return validationResult;

        RecordAttack(attacker, GetHeroLogName(targetHero));
        BattleLogEntry damageLogEntry = DamageHero(targetHero, attacker.Attack, GetMinionLogName(attacker), attacker.Owner, BattleLogEntryType.Damage);
        attacker.SetCanAttack(false);

        CheckGameOver();
        return BuildAttackSuccessResult(damageLogEntry, $"{GetMinionLogName(attacker)} 攻击 {GetHeroLogName(targetHero)}。");
    }

    /// <summary>
    /// 清理双方战场上生命值小于等于 0 的随从。
    /// </summary>
    public void CleanupDeadMinions()
    {
        RemoveDeadMinions(Player);
        RemoveDeadMinions(Enemy);
    }

    /// <summary>
    /// 检查双方英雄是否死亡。
    /// 如果双方同时死亡，Winner 保持 null，表示平局。
    /// </summary>
    public void CheckGameOver()
    {
        if (IsGameOver) return;

        bool playerDead = Player != null && Player.Hero.IsDead;
        bool enemyDead = Enemy != null && Enemy.Hero.IsDead;

        if (!playerDead && !enemyDead) return;

        IsGameOver = true;

        if (playerDead && enemyDead)
        {
            Winner = null;
        }
        else
        {
            Winner = playerDead ? Enemy : Player;
        }

        string message = Winner == null
            ? "游戏结束：双方英雄同时死亡，平局。"
            : $"游戏结束：{GetPlayerLogName(Winner)} 获胜。";

        RecordBattleLog(
            BattleLogEntryType.GameEnded,
            sourcePlayer: Winner,
            message: message,
            setAsLastAction: true);
    }

    /// <summary>
    /// 统一检查随从是否满足攻击条件。
    /// </summary>
    internal GameActionResult ValidateAttack(Minion attacker)
    {
        if (attacker == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidAttacker,
                "攻击者无效。");
        }

        if (IsGameOver)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.GameOver,
                "游戏已经结束，不能继续攻击。");
        }

        if (CurrentPlayer == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.NoCurrentPlayer,
                "当前没有行动玩家。");
        }

        if (attacker.Owner != CurrentPlayer)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.NotCurrentPlayerMinion,
                "只能使用当前玩家自己的随从攻击。");
        }

        if (attacker.IsDead)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.MinionDead,
                $"{GetMinionLogName(attacker)} 已经死亡，不能攻击。");
        }

        if (!attacker.CanAttack)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.MinionCannotAttack,
                $"{GetMinionLogName(attacker)} 现在不能攻击。");
        }

        return GameActionResult.Succeeded();
    }

    /// <summary>
    /// 统一处理随从被召唤成功后的结算。
    /// 当前包含冲锋和最小战吼，后续可以逐步替换成事件系统。
    /// </summary>
    private void ResolveAfterSummon(Minion minion)
    {
        if (minion == null) return;

        ApplySummonKeywords(minion);
        ResolveBattlecry(minion);
    }

    /// <summary>
    /// 判断一个随从是否能成为当前攻击者的攻击目标。
    /// 如果防守方有活着的嘲讽随从，就只能攻击嘲讽随从。
    /// </summary>
    internal GameActionResult ValidateAttackTarget(Minion attacker, Minion target)
    {
        GameActionResult attackerValidationResult = ValidateAttack(attacker);
        if (attackerValidationResult.Failed) return attackerValidationResult;

        if (target == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidTarget,
                "攻击目标无效。");
        }

        if (target.IsDead)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.MinionDead,
                $"{GetMinionLogName(target)} 已经死亡，不能成为攻击目标。");
        }

        Player opponent = GetOpponent(attacker.Owner);
        if (opponent == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidTarget,
                "找不到攻击者的对手。");
        }

        if (target.Owner != opponent)
        {
            return GameActionResult.FailedWith(
                target.Owner == attacker.Owner ? GameActionFailureReason.TargetIsFriendly : GameActionFailureReason.InvalidTarget,
                "不能攻击友方随从。");
        }

        if (HasAliveTauntMinion(opponent))
        {
            if (!target.HasKeyword(KeywordType.Taunt))
            {
                return GameActionResult.FailedWith(
                    GameActionFailureReason.TauntBlocksTarget,
                    "敌方有嘲讽随从，必须优先攻击嘲讽随从。");
            }
        }

        return GameActionResult.Succeeded();
    }

    /// <summary>
    /// 判断一个英雄是否能成为当前攻击者的攻击目标。
    /// </summary>
    internal GameActionResult ValidateAttackHeroTarget(Minion attacker, Hero targetHero)
    {
        GameActionResult attackerValidationResult = ValidateAttack(attacker);
        if (attackerValidationResult.Failed) return attackerValidationResult;

        if (targetHero == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidTarget,
                "攻击目标英雄无效。");
        }

        if (targetHero.IsDead)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidTarget,
                $"{GetHeroLogName(targetHero)} 已经死亡，不能成为攻击目标。");
        }

        Player opponent = GetOpponent(attacker.Owner);
        if (opponent == null)
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.InvalidTarget,
                "找不到攻击者的对手。");
        }

        if (targetHero != opponent.Hero)
        {
            return GameActionResult.FailedWith(
                attacker.Owner != null && targetHero == attacker.Owner.Hero ? GameActionFailureReason.TargetIsFriendly : GameActionFailureReason.InvalidTarget,
                "不能攻击友方英雄。");
        }

        if (HasAliveTauntMinion(opponent))
        {
            return GameActionResult.FailedWith(
                GameActionFailureReason.TauntBlocksTarget,
                "敌方有嘲讽随从，不能直接攻击英雄。");
        }

        return GameActionResult.Succeeded();
    }

    /// <summary>
    /// 判断指定玩家场上是否有仍然存活的嘲讽随从。
    /// </summary>
    internal bool HasAliveTauntMinion(Player owner)
    {
        if (owner == null) return false;
        if (Board == null) return false;

        IReadOnlyList<Minion> minions = Board.GetMinions(owner);
        if (minions == null) return false;

        foreach (Minion minion in minions)
        {
            if (minion == null) continue;
            if (minion.IsDead) continue;

            if (minion.HasKeyword(KeywordType.Taunt))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 处理随从被召唤时立刻生效的关键词。
    /// 当前只处理冲锋：新召唤的随从本回合可以立即攻击。
    /// </summary>
    private void ApplySummonKeywords(Minion minion)
    {
        if (minion == null) return;

        if (minion.HasKeyword(KeywordType.Charge))
        {
            minion.SetCanAttack(true);
        }
    }

    /// <summary>
    /// 处理随从的战吼效果。
    /// 当前阶段先只支持一个最小战吼：对敌方英雄造成伤害。
    /// </summary>
    private void ResolveBattlecry(Minion minion)
    {
        if (minion == null) return;
        if (minion.CardData == null) return;
        if (!minion.CardData.HasBattlecry) return;

        switch (minion.CardData.BattlecryType)
        {
            case BattlecryType.DealDamageToEnemyHero:
                DealBattlecryDamageToEnemyHero(minion);
                break;
            case BattlecryType.DrawCard:
                DrawCardsForBattlecryOwner(minion);
                break;
        }
    }

    /// <summary>
    /// 战吼：对敌方英雄造成伤害。
    /// </summary>
    private void DealBattlecryDamageToEnemyHero(Minion minion)
    {
        if (minion == null || minion.CardData == null) return;

        Player opponent = GetOpponent(minion.Owner);
        if (opponent == null || opponent.Hero == null) return;

        DamageHero(
            opponent.Hero,
            minion.CardData.BattlecryValue,
            $"{GetMinionLogName(minion)} 战吼",
            minion.Owner,
            BattleLogEntryType.Battlecry);
        CheckGameOver();
    }

    /// <summary>
    /// 战吼：为打出这个随从的玩家抽牌。
    /// </summary>
    private void DrawCardsForBattlecryOwner(Minion minion)
    {
        if (minion == null || minion.CardData == null) return;
        if (minion.Owner == null) return;

        int drawCount = minion.CardData.BattlecryValue;
        for (int i = 0; i < drawCount; i++)
        {
            minion.Owner.DrawCard();
        }
    }

    /// <summary>
    /// 注册会影响游戏规则的事件监听。
    /// 当前用于在随从死亡事件发生时结算亡语。
    /// </summary>
    private void SubscribeGameplayEventHandlers()
    {
        if (EventBus == null) return;

        EventBus.Subscribe(GameEventType.MinionDied, ResolveDeathrattleOnMinionDied);
    }

    /// <summary>
    /// 监听随从死亡事件，并尝试结算死亡随从的亡语。
    /// </summary>
    private void ResolveDeathrattleOnMinionDied(GameEvent gameEvent)
    {
        if (gameEvent == null) return;
        if (gameEvent.Type != GameEventType.MinionDied) return;

        ResolveDeathrattle(gameEvent.TargetMinion);
    }

    /// <summary>
    /// 根据死亡随从的卡牌配置结算亡语。
    /// </summary>
    private void ResolveDeathrattle(Minion minion)
    {
        if (minion == null) return;
        if (minion.CardData == null) return;
        if (!minion.CardData.HasDeathrattle) return;

        switch (minion.CardData.DeathrattleType)
        {
            case DeathrattleType.DealDamageToEnemyHero:
                DealDeathrattleDamageToEnemyHero(minion);
                break;
        }
    }

    /// <summary>
    /// 亡语：对死亡随从拥有者的敌方英雄造成伤害。
    /// </summary>
    private void DealDeathrattleDamageToEnemyHero(Minion minion)
    {
        if (minion == null || minion.CardData == null) return;

        Player opponent = GetOpponent(minion.Owner);
        if (opponent == null || opponent.Hero == null) return;

        DamageHero(
            opponent.Hero,
            minion.CardData.DeathrattleValue,
            $"{GetMinionLogName(minion)} 亡语",
            minion.Owner,
            BattleLogEntryType.Deathrattle);
        CheckGameOver();
    }

    /// <summary>
    /// 发布随从死亡事件。
    /// 死亡随从是事件目标，所以填入 TargetMinion。
    /// </summary>
    private void PublishMinionDied(Minion minion)
    {
        if (EventBus == null) return;
        if (minion == null) return;

        GameEvent gameEvent = new GameEvent(
            GameEventType.MinionDied,
            targetMinion: minion);

        EventBus.Publish(gameEvent);
    }

    /// <summary>
    /// 从指定玩家的战场列表中倒序移除死亡随从。
    /// 倒序遍历可以避免删除元素时影响还没检查的索引。
    /// </summary>
    private void RemoveDeadMinions(Player owner)
    {
        if (owner == null) return;

        IReadOnlyList<Minion> minions = Board.GetMinions(owner);
        if (minions == null) return;

        for (int i = minions.Count - 1; i >= 0; i--)
        {
            Minion minion = minions[i];
            if (minion.IsDead)
            {
                RecordMinionDied(minion);
                PublishMinionDied(minion);
                Board.RemoveMinion(minion);
            }
        }
    }
}
