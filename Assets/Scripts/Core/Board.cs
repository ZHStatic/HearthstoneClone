using System.Collections.Generic;

/// <summary>
/// 战场 - 管理双方已经召唤上场的随从。
/// 这里只负责随从站位列表，不处理攻击、死亡结算和 UI 刷新。
/// </summary>
public class Board
{
    private readonly Player player;
    private readonly Player enemy;
    private readonly List<Minion> playerMinions;
    private readonly List<Minion> enemyMinions;
    private readonly int maxMinionsPerSide;

    public int MaxMinionsPerSide => maxMinionsPerSide;

    public Board(Player player, Player enemy, int maxMinionsPerSide = 7)
    {
        this.player = player;
        this.enemy = enemy;
        this.maxMinionsPerSide = maxMinionsPerSide > 0 ? maxMinionsPerSide : 7;

        playerMinions = new List<Minion>();
        enemyMinions = new List<Minion>();
    }

    /// <summary>
    /// 判断指定玩家的战场是否还有空位。
    /// </summary>
    public bool CanSummon(Player owner)
    {
        List<Minion> minions = GetMutableMinions(owner);
        return minions != null && minions.Count < maxMinionsPerSide;
    }

    /// <summary>
    /// 将一个随从召唤到指定玩家的战场。
    /// </summary>
    /// <returns>召唤成功返回 true，失败返回 false。</returns>
    public bool SummonMinion(Minion minion)
    {
        if (minion == null) return false;

        List<Minion> minions = GetMutableMinions(minion.Owner);
        if (minions == null) return false;
        if (minions.Count >= maxMinionsPerSide) return false;
        if (minions.Contains(minion)) return false;

        minions.Add(minion);
        return true;
    }

    /// <summary>
    /// 从战场移除一个随从。
    /// </summary>
    /// <returns>成功找到并移除返回 true，否则返回 false。</returns>
    public bool RemoveMinion(Minion minion)
    {
        if (minion == null) return false;

        List<Minion> minions = GetMutableMinions(minion.Owner);
        if (minions == null) return false;

        return minions.Remove(minion);
    }

    /// <summary>
    /// 获取指定玩家场上的随从列表。
    /// 外部只能读取列表，不能直接增删随从。
    /// </summary>
    public IReadOnlyList<Minion> GetMinions(Player owner)
    {
        return GetMutableMinions(owner);
    }

    private List<Minion> GetMutableMinions(Player owner)
    {
        if (owner == player) return playerMinions;
        if (owner == enemy) return enemyMinions;

        return null;
    }
}
