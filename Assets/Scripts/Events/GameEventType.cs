/// <summary>
/// 游戏事件类型。
/// 只描述“发生了什么事”，具体数据由 GameEvent 承载。
/// </summary>
public enum GameEventType
{
    /// <summary>
    /// 有卡牌被打出。
    /// </summary>
    CardPlayed,

    /// <summary>
    /// 有随从被召唤到战场。
    /// </summary>
    MinionSummoned,

    /// <summary>
    /// 有随从死亡。
    /// </summary>
    MinionDied,

    /// <summary>
    /// 一个玩家的回合开始。
    /// </summary>
    TurnStarted,

    /// <summary>
    /// 一个玩家的回合结束。
    /// </summary>
    TurnEnded
}
