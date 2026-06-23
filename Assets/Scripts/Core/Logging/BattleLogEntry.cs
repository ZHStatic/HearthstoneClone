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
    public BattleLogEntryType EntryType { get; private set; }
    public int TurnNumber { get; private set; }
    public string SourcePlayerName { get; private set; }
    public string TargetPlayerName { get; private set; }
    public string SourceName { get; private set; }
    public string TargetName { get; private set; }
    public int AttemptedAmount { get; private set; }
    public int ActualAmount { get; private set; }
    public string Message { get; private set; }

    public bool WasPrevented => AttemptedAmount > 0 && ActualAmount == 0;

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
