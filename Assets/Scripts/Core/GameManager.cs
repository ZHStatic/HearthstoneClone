using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对局管理器。
/// 负责一局游戏的初始化、回合切换、出牌、攻击、死亡清理和胜负判断。
/// 之后 UI 可以调用它的方法，但它本身不直接依赖 UI。
/// </summary>
public class GameManager : MonoBehaviour
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
        if (!CanPlayCard(card)) return false;
        if (card.CardData.CardType != CardType.Minion) return false;
        if (Board == null) return false;
        if (!Board.CanSummon(CurrentPlayer)) return false;

        bool played = CurrentPlayer.PlayCard(card);
        if (!played) return false;

        Minion minion = new Minion(card.CardData, CurrentPlayer);
        bool summoned = Board.SummonMinion(minion);
        if (!summoned) return false;

        PublishCardPlayed(card);
        PublishMinionSummoned(minion);
        ResolveAfterSummon(minion);
        return true;
    }

    /// <summary>
    /// 尝试把当前玩家手牌中的单目标伤害法术打到一个随从身上。
    /// 阶段 2.1 先只支持 SpellDamage，不处理治疗、Buff 和事件系统。
    /// </summary>
    public bool TryPlaySpellCardOnMinion(Card card, Minion target)
    {
        if (!CanPlayCard(card)) return false;
        if (card.CardData.CardType != CardType.Spell) return false;
        if (!CanTargetMinion(card.CardData, target)) return false;

        bool played = CurrentPlayer.PlayCard(card);
        if (!played) return false;

        PublishCardPlayed(card);
        target.TakeDamage(card.CardData.SpellDamage);

        CleanupDeadMinions();
        CheckGameOver();
        return true;
    }

    /// <summary>
    /// 尝试把当前玩家手牌中的单目标伤害法术打到一个英雄身上。
    /// 阶段 2.1 先只支持 SpellDamage，不处理治疗、Buff 和事件系统。
    /// </summary>
    public bool TryPlaySpellCardOnHero(Card card, Hero targetHero)
    {
        if (!CanPlayCard(card)) return false;
        if (card.CardData.CardType != CardType.Spell) return false;
        if (!CanTargetHero(card.CardData, targetHero)) return false;

        bool played = CurrentPlayer.PlayCard(card);
        if (!played) return false;

        PublishCardPlayed(card);
        targetHero.TakeDamage(card.CardData.SpellDamage);

        CleanupDeadMinions();
        CheckGameOver();
        return true;
    }

    /// <summary>
    /// 尝试让一个随从攻击另一个随从。
    /// 双方会互相造成等于自身攻击力的伤害，然后清理死亡随从。
    /// </summary>
    public bool TryAttackMinion(Minion attacker, Minion target)
    {
        if (!CanAttack(attacker)) return false;
        if (!IsValidAttackTarget(attacker, target)) return false;

        attacker.TakeDamage(target.Attack);
        target.TakeDamage(attacker.Attack);
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

        targetHero.TakeDamage(attacker.Attack);
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

        opponent.Hero.TakeDamage(minion.CardData.BattlecryValue);
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

        opponent.Hero.TakeDamage(minion.CardData.DeathrattleValue);
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
    /// 通用出牌检查：只判断一张手牌能不能被当前玩家支付并打出。
    /// 具体是召唤随从还是结算法术，由各自的 TryPlay 方法继续判断。
    /// </summary>
    private bool CanPlayCard(Card card)
    {
        if (card == null) return false;
        if (card.CardData == null) return false;
        if (IsGameOver) return false;
        if (CurrentPlayer == null) return false;
        if (!CurrentPlayer.Hand.Contains(card)) return false;
        if (card.CurrentCost > CurrentPlayer.CurrentMana) return false;

        return true;
    }

    /// <summary>
    /// 根据法术目标类型判断目标随从是否合法。
    /// </summary>
    private bool CanTargetMinion(CardData spellData, Minion target)
    {
        if (spellData == null) return false;
        if (spellData.CardType != CardType.Spell) return false;
        if (target == null) return false;
        if (target.IsDead) return false;

        Player targetOwner = target.Owner;
        if (targetOwner != Player && targetOwner != Enemy) return false;

        Player opponent = GetOpponent(CurrentPlayer);

        switch (spellData.SpellTargetType)
        {
            case SpellTargetType.AnyCharacter:
            case SpellTargetType.Minion:
                return true;
            case SpellTargetType.EnemyCharacter:
            case SpellTargetType.EnemyMinion:
                return targetOwner == opponent;
            case SpellTargetType.FriendlyCharacter:
            case SpellTargetType.FriendlyMinion:
                return targetOwner == CurrentPlayer;
            default:
                return false;
        }
    }

    /// <summary>
    /// 根据法术目标类型判断目标英雄是否合法。
    /// </summary>
    private bool CanTargetHero(CardData spellData, Hero targetHero)
    {
        if (spellData == null) return false;
        if (spellData.CardType != CardType.Spell) return false;
        if (targetHero == null) return false;
        if (targetHero.IsDead) return false;

        Player targetOwner = null;
        if (Player != null && targetHero == Player.Hero)
        {
            targetOwner = Player;
        }
        else if (Enemy != null && targetHero == Enemy.Hero)
        {
            targetOwner = Enemy;
        }

        if (targetOwner == null) return false;

        Player opponent = GetOpponent(CurrentPlayer);

        return spellData.SpellTargetType switch
        {
            SpellTargetType.AnyCharacter => true,
            SpellTargetType.EnemyCharacter => targetOwner == opponent,
            SpellTargetType.FriendlyCharacter => targetOwner == CurrentPlayer,
            _ => false,
        };
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
                PublishMinionDied(minion);
                Board.RemoveMinion(minion);
            }
        }
    }
}
