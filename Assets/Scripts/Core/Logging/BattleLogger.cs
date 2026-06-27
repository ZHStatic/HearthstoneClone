using System.Collections.Generic;

/// <summary>
/// 战斗日志记录器。
/// 保存本局发生过的战斗日志；当前只提供记录、只读查看和清空能力。
/// </summary>
public class BattleLogger
{
    private readonly List<BattleLogEntry> entries = new List<BattleLogEntry>();

    /// <summary>
    /// 本局已经记录的全部日志。外部只能读取，不能直接增删。
    /// </summary>
    public IReadOnlyList<BattleLogEntry> Entries => entries;

    /// <summary>
    /// 最近一条日志；当前还没有日志时返回 null。
    /// </summary>
    public BattleLogEntry LastEntry => entries.Count > 0 ? entries[entries.Count - 1] : null;

    /// <summary>
    /// 当前已经记录的日志数量。
    /// </summary>
    public int Count => entries.Count;

    /// <summary>
    /// 添加一条已经创建好的日志。
    /// </summary>
    public void Add(BattleLogEntry entry)
    {
        if (entry == null) return;

        entries.Add(entry);
    }

    /// <summary>
    /// 创建并添加一条日志。
    /// </summary>
    public BattleLogEntry Add(
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
        BattleLogEntry entry = new BattleLogEntry(
            entryType,
            turnNumber,
            sourcePlayerName,
            targetPlayerName,
            sourceName,
            targetName,
            attemptedAmount,
            actualAmount,
            message);

        entries.Add(entry);
        return entry;
    }

    /// <summary>
    /// 清空当前记录。
    /// </summary>
    public void Clear()
    {
        entries.Clear();
    }
}
