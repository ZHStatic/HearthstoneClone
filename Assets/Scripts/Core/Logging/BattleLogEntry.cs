/// <summary>
/// 战斗日志类型。
/// 只描述这条日志记录的事件类别，不负责执行规则。
/// </summary>
public enum BattleLogEntryType
{
    TurnStarted,
    TurnEnded,
    CardPlayed,
    MinionSummoned,
    Attack,
    Spell,
    Battlecry,
    Deathrattle,
    Damage,
    DivineShieldPrevented,
    MinionDied,
    GameEnded
}

/// <summary>
/// 一条战斗日志快照。
/// 它只保存发生时的文本和数值，不保存 Player、Card、Minion 等运行时引用。
/// </summary>
public class BattleLogEntry
{
    /// <summary>
    /// 这条日志对应的事件类别，例如攻击、法术、亡语或游戏结束。
    /// </summary>
    public BattleLogEntryType EntryType { get; private set; }

    /// <summary>
    /// 日志发生时的回合数。
    /// </summary>
    public int TurnNumber { get; private set; }

    /// <summary>
    /// 行动来源玩家的显示名称；没有来源玩家时为空字符串。
    /// </summary>
    public string SourcePlayerName { get; private set; }

    /// <summary>
    /// 目标所属玩家的显示名称；没有目标玩家时为空字符串。
    /// </summary>
    public string TargetPlayerName { get; private set; }

    /// <summary>
    /// 行动来源的显示名称，例如卡牌名、随从名或英雄名。
    /// </summary>
    public string SourceName { get; private set; }

    /// <summary>
    /// 行动目标的显示名称，例如随从名或英雄名。
    /// </summary>
    public string TargetName { get; private set; }

    /// <summary>
    /// 规则尝试结算的数值，例如尝试造成的伤害。
    /// </summary>
    public int AttemptedAmount { get; private set; }

    /// <summary>
    /// 规则最终实际结算的数值，例如被圣盾抵消后可能为 0。
    /// </summary>
    public int ActualAmount { get; private set; }

    /// <summary>
    /// 面向玩家或调试输出的日志文本。
    /// </summary>
    public string Message { get; private set; }

    public BattleLogEntry(
        BattleLogEntryType entryType,
        int turnNumber,
        string sourcePlayerName = "",
        string targetPlayerName = "",
        string sourceName = "",
        string targetName = "",
        int attemptedAmount = 0,
        int actualAmount = 0,
        string message = "")
    {
        EntryType = entryType;
        TurnNumber = turnNumber;
        SourcePlayerName = sourcePlayerName ?? "";
        TargetPlayerName = targetPlayerName ?? "";
        SourceName = sourceName ?? "";
        TargetName = targetName ?? "";
        AttemptedAmount = attemptedAmount;
        ActualAmount = actualAmount;
        Message = message ?? "";
    }

    public override string ToString()
    {
        return Message;
    }
}
