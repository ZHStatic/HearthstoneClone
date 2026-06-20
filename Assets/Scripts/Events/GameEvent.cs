/// <summary>
/// 游戏事件数据。
/// 只记录事件类型和相关上下文，不执行规则。
/// </summary>
public class GameEvent
{
    /// <summary>
    /// 发生了什么事件。
    /// </summary>
    public GameEventType Type { get; private set; }

    /// <summary>
    /// 事件来源玩家，可以为空。
    /// </summary>
    public Player SourcePlayer { get; private set; }

    /// <summary>
    /// 事件目标玩家，可以为空。
    /// </summary>
    public Player TargetPlayer { get; private set; }

    /// <summary>
    /// 事件来源卡牌，可以为空。
    /// </summary>
    public Card SourceCard { get; private set; }

    /// <summary>
    /// 事件来源随从，可以为空。
    /// </summary>
    public Minion SourceMinion { get; private set; }

    /// <summary>
    /// 事件目标随从，可以为空。
    /// </summary>
    public Minion TargetMinion { get; private set; }

    /// <summary>
    /// 事件相关数值；不用时为 0。
    /// </summary>
    public int Amount { get; private set; }

    /// <summary>
    /// 创建一条游戏事件。
    /// </summary>
    public GameEvent(
        GameEventType type,
        Player sourcePlayer = null,
        Player targetPlayer = null,
        Card sourceCard = null,
        Minion sourceMinion = null,
        Minion targetMinion = null,
        int amount = 0)
    {
        Type = type;
        SourcePlayer = sourcePlayer;
        TargetPlayer = targetPlayer;
        SourceCard = sourceCard;
        SourceMinion = sourceMinion;
        TargetMinion = targetMinion;
        Amount = amount;
    }
}
