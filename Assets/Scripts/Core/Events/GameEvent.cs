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
    /// 事件目标随从。当前用于表示死亡随从。
    /// </summary>
    public Minion TargetMinion { get; private set; }

    /// <summary>
    /// 创建一条游戏事件。
    /// </summary>
    public GameEvent(
        GameEventType type,
        Minion targetMinion = null)
    {
        Type = type;
        TargetMinion = targetMinion;
    }
}
