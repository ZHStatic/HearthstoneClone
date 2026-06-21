using System.Collections.Generic;

/// <summary>
/// 战斗日志记录器。
/// 保存本局发生过的战斗日志，并提供简单查询和统计。
/// </summary>
public class BattleLogger
{
    private readonly List<BattleLogEntry> entries = new List<BattleLogEntry>();

    public IReadOnlyList<BattleLogEntry> Entries => entries;
    public BattleLogEntry LastEntry => entries.Count > 0 ? entries[entries.Count - 1] : null;
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

    /// <summary>
    /// 获取最近 count 条日志。
    /// </summary>
    public List<BattleLogEntry> GetLatestEntries(int count)
    {
        List<BattleLogEntry> result = new List<BattleLogEntry>();
        if (count <= 0) return result;

        int startIndex = entries.Count - count;
        if (startIndex < 0)
        {
            startIndex = 0;
        }

        for (int i = startIndex; i < entries.Count; i++)
        {
            result.Add(entries[i]);
        }

        return result;
    }

    /// <summary>
    /// 按日志类型筛选。
    /// </summary>
    public List<BattleLogEntry> GetEntriesByType(BattleLogEntryType entryType)
    {
        List<BattleLogEntry> result = new List<BattleLogEntry>();

        foreach (BattleLogEntry entry in entries)
        {
            if (entry.EntryType == entryType)
            {
                result.Add(entry);
            }
        }

        return result;
    }

    /// <summary>
    /// 按来源玩家或目标玩家筛选。
    /// </summary>
    public List<BattleLogEntry> GetEntriesByPlayer(string playerName)
    {
        List<BattleLogEntry> result = new List<BattleLogEntry>();
        if (string.IsNullOrWhiteSpace(playerName)) return result;

        foreach (BattleLogEntry entry in entries)
        {
            if (entry.SourcePlayerName == playerName || entry.TargetPlayerName == playerName)
            {
                result.Add(entry);
            }
        }

        return result;
    }

    /// <summary>
    /// 统计某个来源造成的实际数值。
    /// </summary>
    public int GetTotalActualAmountBySource(string sourceName)
    {
        if (string.IsNullOrWhiteSpace(sourceName)) return 0;

        int total = 0;
        foreach (BattleLogEntry entry in entries)
        {
            if (entry.SourceName == sourceName)
            {
                total += entry.ActualAmount;
            }
        }

        return total;
    }

    /// <summary>
    /// 统计某个目标受到的实际数值。
    /// </summary>
    public int GetTotalActualAmountToTarget(string targetName)
    {
        if (string.IsNullOrWhiteSpace(targetName)) return 0;

        int total = 0;
        foreach (BattleLogEntry entry in entries)
        {
            if (entry.TargetName == targetName)
            {
                total += entry.ActualAmount;
            }
        }

        return total;
    }

    /// <summary>
    /// 把最近 count 条日志拼成多行文本。
    /// </summary>
    public string BuildRecentText(int count)
    {
        List<BattleLogEntry> latestEntries = GetLatestEntries(count);
        if (latestEntries.Count == 0) return "";

        List<string> messages = new List<string>();
        foreach (BattleLogEntry entry in latestEntries)
        {
            messages.Add(entry.Message);
        }

        return string.Join("\n", messages);
    }
}
