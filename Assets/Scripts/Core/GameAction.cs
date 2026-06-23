/// <summary>
/// 一条具体的游戏动作。
/// 它只记录“玩家想做什么”，不判断合法性，也不直接修改游戏状态。
/// 例如：某个玩家打出一张牌、某个随从攻击目标、某个玩家结束回合。
/// 后续玩家输入、动作枚举和阶段 3 AI 都可以复用这个数据结构。
/// </summary>
public class GameAction
{
    /// <summary>
    /// 动作类型，用来区分这条动作是出牌、施法、攻击还是结束回合。
    /// </summary>
    public GameActionType ActionType { get; private set; }

    /// <summary>
    /// 执行动作的玩家。
    /// 当前阶段通常等于 GameManager.CurrentPlayer。
    /// </summary>
    public Player Actor { get; private set; }

    /// <summary>
    /// 出牌或施法动作使用的手牌。
    /// 攻击和结束回合动作不会使用这个字段。
    /// </summary>
    public Card Card { get; private set; }

    /// <summary>
    /// 攻击动作中的攻击随从。
    /// 出牌、施法和结束回合动作不会使用这个字段。
    /// </summary>
    public Minion Attacker { get; private set; }

    /// <summary>
    /// 法术或攻击动作中的目标随从。
    /// 目标是英雄时不会使用这个字段。
    /// </summary>
    public Minion TargetMinion { get; private set; }

    /// <summary>
    /// 法术或攻击动作中的目标英雄。
    /// 目标是随从时不会使用这个字段。
    /// </summary>
    public Hero TargetHero { get; private set; }

    /// <summary>
    /// 私有构造函数，强制外部通过下面的 Create... 方法创建动作。
    /// 这样每种动作需要填哪些字段会更清楚。
    /// </summary>
    private GameAction(
        GameActionType actionType,
        Player actor,
        Card card,
        Minion attacker,
        Minion targetMinion,
        Hero targetHero)
    {
        ActionType = actionType;
        Actor = actor;
        Card = card;
        Attacker = attacker;
        TargetMinion = targetMinion;
        TargetHero = targetHero;
    }

    /// <summary>
    /// 创建“打出随从牌”动作。
    /// 需要执行玩家和要打出的手牌，不需要目标。
    /// </summary>
    public static GameAction CreatePlayMinionCard(Player actor, Card card)
    {
        return new GameAction(
            GameActionType.PlayMinionCard,
            actor,
            card,
            null,
            null,
            null);
    }

    /// <summary>
    /// 创建“对随从释放法术”动作。
    /// 需要执行玩家、法术牌和目标随从。
    /// </summary>
    public static GameAction CreatePlaySpellOnMinion(Player actor, Card card, Minion targetMinion)
    {
        return new GameAction(
            GameActionType.PlaySpellOnMinion,
            actor,
            card,
            null,
            targetMinion,
            null);
    }

    /// <summary>
    /// 创建“对英雄释放法术”动作。
    /// 需要执行玩家、法术牌和目标英雄。
    /// </summary>
    public static GameAction CreatePlaySpellOnHero(Player actor, Card card, Hero targetHero)
    {
        return new GameAction(
            GameActionType.PlaySpellOnHero,
            actor,
            card,
            null,
            null,
            targetHero);
    }

    /// <summary>
    /// 创建“随从攻击随从”动作。
    /// 需要执行玩家、攻击随从和目标随从。
    /// </summary>
    public static GameAction CreateAttackMinion(Player actor, Minion attacker, Minion targetMinion)
    {
        return new GameAction(
            GameActionType.AttackMinion,
            actor,
            null,
            attacker,
            targetMinion,
            null);
    }

    /// <summary>
    /// 创建“随从攻击英雄”动作。
    /// 需要执行玩家、攻击随从和目标英雄。
    /// </summary>
    public static GameAction CreateAttackHero(Player actor, Minion attacker, Hero targetHero)
    {
        return new GameAction(
            GameActionType.AttackHero,
            actor,
            null,
            attacker,
            null,
            targetHero);
    }

    /// <summary>
    /// 创建“结束回合”动作。
    /// 只需要执行玩家，不需要卡牌或目标。
    /// </summary>
    public static GameAction CreateEndTurn(Player actor)
    {
        return new GameAction(
            GameActionType.EndTurn,
            actor,
            null,
            null,
            null,
            null);
    }
}
