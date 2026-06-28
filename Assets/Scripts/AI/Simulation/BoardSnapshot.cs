using System.Collections.Generic;

/// <summary>
/// AI 模拟用的战场快照。
/// 它保存双方场上随从的轻量副本，不引用真实战场列表。
/// </summary>
public class BoardSnapshot
{
    private readonly List<MinionSnapshot> playerMinions;
    private readonly List<MinionSnapshot> enemyMinions;

    /// <summary>
    /// 玩家侧场上随从快照。
    /// </summary>
    public IReadOnlyList<MinionSnapshot> PlayerMinions => playerMinions;

    /// <summary>
    /// 敌人侧场上随从快照。
    /// </summary>
    public IReadOnlyList<MinionSnapshot> EnemyMinions => enemyMinions;

    public BoardSnapshot(IReadOnlyList<MinionSnapshot> playerMinions, IReadOnlyList<MinionSnapshot> enemyMinions)
    {
        this.playerMinions = CopyMinions(playerMinions);
        this.enemyMinions = CopyMinions(enemyMinions);
    }

    /// <summary>
    /// 从真实 Board 复制双方场面快照。
    /// </summary>
    public static BoardSnapshot FromBoard(Board board, Player player, Player enemy)
    {
        if (board == null)
        {
            return new BoardSnapshot(null, null);
        }

        List<MinionSnapshot> playerSnapshots = CreateMinionSnapshots(board.GetMinions(player));
        List<MinionSnapshot> enemySnapshots = CreateMinionSnapshots(board.GetMinions(enemy));

        return new BoardSnapshot(playerSnapshots, enemySnapshots);
    }

    /// <summary>
    /// 根据玩家索引获取对应的随从列表。
    /// </summary>
    public IReadOnlyList<MinionSnapshot> GetMinions(int playerIndex)
    {
        return playerIndex == GameStateSnapshot.EnemyIndex ? EnemyMinions : PlayerMinions;
    }

    /// <summary>
    /// 获取当前行动方的场面随从。
    /// </summary>
    public IReadOnlyList<MinionSnapshot> GetCurrentPlayerMinions(GameStateSnapshot state)
    {
        if (state == null) return PlayerMinions;

        return GetMinions(state.CurrentPlayerIndex);
    }

    /// <summary>
    /// 获取当前行动方对手的场面随从。
    /// </summary>
    public IReadOnlyList<MinionSnapshot> GetOpponentMinions(GameStateSnapshot state)
    {
        if (state == null) return EnemyMinions;

        int opponentIndex = state.CurrentPlayerIndex == GameStateSnapshot.EnemyIndex
            ? GameStateSnapshot.PlayerIndex
            : GameStateSnapshot.EnemyIndex;

        return GetMinions(opponentIndex);
    }

    private static List<MinionSnapshot> CreateMinionSnapshots(IReadOnlyList<Minion> minions)
    {
        List<MinionSnapshot> snapshots = new List<MinionSnapshot>();
        if (minions == null) return snapshots;

        for (int i = 0; i < minions.Count; i++)
        {
            MinionSnapshot snapshot = MinionSnapshot.FromMinion(minions[i]);
            if (snapshot == null) continue;

            snapshots.Add(snapshot);
        }

        return snapshots;
    }

    private static List<MinionSnapshot> CopyMinions(IReadOnlyList<MinionSnapshot> minions)
    {
        List<MinionSnapshot> copy = new List<MinionSnapshot>();
        if (minions == null) return copy;

        for (int i = 0; i < minions.Count; i++)
        {
            MinionSnapshot minion = minions[i];
            if (minion == null) continue;

            copy.Add(minion);
        }

        return copy;
    }
}
