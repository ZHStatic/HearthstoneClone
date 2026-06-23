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
    [SerializeField] private bool logGameEvents = true;

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

        Player = new Player(playerDeckData, "Player");
        Enemy = new Player(enemyDeckData, "Enemy");
        Board = new Board(Player, Enemy);

        CurrentPlayer = Player;
        Winner = null;
        TurnNumber = 0;
        IsGameOver = false;

        SubscribeGameplayEventHandlers();
        SubscribeDebugEventLogs();
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
        if (minions == null) return;

        foreach (Minion minion in minions)
        {
            minion.SetCanAttack(true);
        }
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
    /// 尝试把当前玩家手牌中的一张随从牌召唤到战场。
    /// 目前阶段所有 CardData 都先当作随从牌处理，法术和武器之后再扩展。
    /// </summary>
    public bool TryPlayMinionCard(Card card)
    {
        return TryPlayMinionCardDetailed(card).Success;
    }

    /// <summary>
    /// 尝试打出随从牌，并返回更详细的操作结果。
    /// 旧的 TryPlayMinionCard 仍然保留，方便现有 UI 继续只读取 bool。
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

        PublishCardPlayed(card);
        RecordCardPlayed(card, CurrentPlayer);
        PublishMinionSummoned(minion);
        RecordMinionSummoned(minion);
        ResolveAfterSummon(minion);
        return GameActionResult.Succeeded($"打出 {GetCardLogName(card)}。");
    }

    /// <summary>
    /// 检查当前玩家能否打出这张随从牌。
    /// 这里只做条件判断，不扣法力、不移除手牌、不创建随从。
    /// </summary>
    private GameActionResult ValidatePlayMinionCard(Card card)
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
    /// 阶段 2.1 先只支持 SpellDamage，不处理治疗、Buff 和事件系统。
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

        PublishCardPlayed(card);
        RecordCardPlayed(card, CurrentPlayer);
        BattleLogEntry spellLogEntry = DamageMinion(target, card.CardData.SpellDamage, GetCardLogName(card), CurrentPlayer, BattleLogEntryType.Spell);

        CleanupDeadMinions();
        CheckGameOver();
        return BuildSpellSuccessResult(spellLogEntry, $"{GetCardLogName(card)} 对 {GetMinionLogName(target)} 释放成功。");
    }

    /// <summary>
    /// 尝试把当前玩家手牌中的单目标伤害法术打到一个英雄身上。
    /// 阶段 2.1 先只支持 SpellDamage，不处理治疗、Buff 和事件系统。
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

        PublishCardPlayed(card);
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
    private GameActionResult ValidatePlaySpellCard(Card card)
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
    private GameActionResult ValidateSpellTargetMinion(CardData spellData, Minion target)
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
    private GameActionResult ValidateSpellTargetHero(CardData spellData, Hero targetHero)
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
    /// 尝试让一个随从攻击另一个随从。
    /// 双方会互相造成等于自身攻击力的伤害，然后清理死亡随从。
    /// </summary>
    public bool TryAttackMinion(Minion attacker, Minion target)
    {
        if (!CanAttack(attacker)) return false;
        if (!IsValidAttackTarget(attacker, target)) return false;

        RecordAttack(attacker, GetMinionLogName(target));
        DamageMinion(attacker, target.Attack, GetMinionLogName(target), target.Owner, BattleLogEntryType.Damage);
        DamageMinion(target, attacker.Attack, GetMinionLogName(attacker), attacker.Owner, BattleLogEntryType.Damage);
        attacker.SetCanAttack(false);

        CleanupDeadMinions();
        CheckGameOver();
        return true;
    }

    /// <summary>
    /// 尝试让一个随从攻击对方英雄。
    /// 不能攻击自己的英雄，也不能攻击不属于本局对手的英雄。
    /// </summary>
    public bool TryAttackHero(Minion attacker, Hero targetHero)
    {
        if (!CanAttack(attacker)) return false;
        if (targetHero == null) return false;

        Player opponent = GetOpponent(attacker.Owner);
        if (opponent == null) return false;
        if (targetHero != opponent.Hero) return false;
        if (HasAliveTauntMinion(opponent)) return false;

        RecordAttack(attacker, GetHeroLogName(targetHero));
        DamageHero(targetHero, attacker.Attack, GetMinionLogName(attacker), attacker.Owner, BattleLogEntryType.Damage);
        attacker.SetCanAttack(false);

        CheckGameOver();
        return true;
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
    private bool CanAttack(Minion attacker)
    {
        if (attacker == null) return false;
        if (IsGameOver) return false;
        if (attacker.Owner != CurrentPlayer) return false;
        if (!attacker.CanAttack) return false;
        if (attacker.IsDead) return false;

        return true;
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
    private bool IsValidAttackTarget(Minion attacker, Minion target)
    {
        if (attacker == null) return false;
        if (target == null) return false;
        if (target.IsDead) return false;

        Player opponent = GetOpponent(attacker.Owner);
        if (opponent == null) return false;
        if (target.Owner != opponent) return false;

        if (HasAliveTauntMinion(opponent))
        {
            return target.HasKeyword(KeywordType.Taunt);
        }

        return true;
    }

    /// <summary>
    /// 判断指定玩家场上是否有仍然存活的嘲讽随从。
    /// </summary>
    private bool HasAliveTauntMinion(Player owner)
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
    /// 发布卡牌打出事件。
    /// </summary>
    private void PublishCardPlayed(Card card)
    {
        if (EventBus == null) return;
        if (card == null) return;

        GameEvent gameEvent = new GameEvent(
            GameEventType.CardPlayed,
            sourcePlayer: CurrentPlayer,
            sourceCard: card);

        EventBus.Publish(gameEvent);
    }

    /// <summary>
    /// 发布随从召唤事件。
    /// </summary>
    private void PublishMinionSummoned(Minion minion)
    {
        if (EventBus == null) return;
        if (minion == null) return;

        GameEvent gameEvent = new GameEvent(
            GameEventType.MinionSummoned,
            sourcePlayer: minion.Owner,
            targetPlayer: minion.Owner,
            targetMinion: minion);

        EventBus.Publish(gameEvent);
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
            targetPlayer: minion.Owner,
            targetMinion: minion);

        EventBus.Publish(gameEvent);
    }

    /// <summary>
    /// 订阅调试用事件日志。
    /// </summary>
    private void SubscribeDebugEventLogs()
    {
        if (!logGameEvents) return;
        if (EventBus == null) return;

        EventBus.Subscribe(GameEventType.CardPlayed, LogGameEvent);
        EventBus.Subscribe(GameEventType.MinionSummoned, LogGameEvent);
        EventBus.Subscribe(GameEventType.MinionDied, LogGameEvent);
    }

    /// <summary>
    /// 在 Console 中打印事件系统是否被触发。
    /// </summary>
    private void LogGameEvent(GameEvent gameEvent)
    {
        if (gameEvent == null) return;

        string cardName = gameEvent.SourceCard != null && gameEvent.SourceCard.CardData != null
            ? gameEvent.SourceCard.CardData.CardName
            : "None";

        string minionName = gameEvent.TargetMinion != null && gameEvent.TargetMinion.CardData != null
            ? gameEvent.TargetMinion.CardData.CardName
            : "None";

        Debug.Log($"GameEvent: {gameEvent.Type}, Card: {cardName}, Minion: {minionName}");
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
