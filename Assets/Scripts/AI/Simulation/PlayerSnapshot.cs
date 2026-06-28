/// <summary>
/// AI 模拟用的玩家状态快照。
/// 它只保存模拟和评估需要的轻量数据，不保存真实手牌对象或牌库对象。
/// </summary>
public class PlayerSnapshot
{
    /// <summary>
    /// 英雄当前血量。
    /// </summary>
    public int HeroHealth { get; private set; }

    /// <summary>
    /// 英雄最大血量。
    /// </summary>
    public int HeroMaxHealth { get; private set; }

    /// <summary>
    /// 当前剩余法力。
    /// </summary>
    public int CurrentMana { get; private set; }

    /// <summary>
    /// 当前最大法力。
    /// </summary>
    public int MaxMana { get; private set; }

    /// <summary>
    /// 当前手牌数量。
    /// </summary>
    public int HandCount { get; private set; }

    /// <summary>
    /// 当前牌库数量。
    /// </summary>
    public int DeckCount { get; private set; }

    public PlayerSnapshot(int heroHealth, int heroMaxHealth, int currentMana, int maxMana, int handCount, int deckCount)
    {
        HeroMaxHealth = ClampNonNegative(heroMaxHealth);
        HeroHealth = Clamp(heroHealth, 0, HeroMaxHealth);
        MaxMana = ClampNonNegative(maxMana);
        CurrentMana = Clamp(currentMana, 0, MaxMana);
        HandCount = ClampNonNegative(handCount);
        DeckCount = ClampNonNegative(deckCount);
    }

    /// <summary>
    /// 从真实 Player 复制一份轻量快照。
    /// </summary>
    public static PlayerSnapshot FromPlayer(Player player)
    {
        if (player == null)
        {
            return new PlayerSnapshot(0, 0, 0, 0, 0, 0);
        }

        int heroHealth = player.Hero != null ? player.Hero.CurrentHealth : 0;
        int heroMaxHealth = player.Hero != null ? player.Hero.MaxHealth : 0;
        int handCount = player.Hand != null ? player.Hand.Count : 0;
        int deckCount = player.Deck != null ? player.Deck.Count : 0;

        return new PlayerSnapshot(
            heroHealth,
            heroMaxHealth,
            player.CurrentMana,
            player.MaxMana,
            handCount,
            deckCount);
    }

    private static int ClampNonNegative(int value)
    {
        return value < 0 ? 0 : value;
    }

    private static int Clamp(int value, int min, int max)
    {
        if (max < min) return min;
        if (value < min) return min;
        if (value > max) return max;

        return value;
    }
}
