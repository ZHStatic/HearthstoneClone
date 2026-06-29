using System.Collections.Generic;

/// <summary>
/// AI 模拟用的玩家状态快照。
/// 它只保存模拟和评估需要的轻量数据，不引用真实手牌对象或牌库对象。
/// </summary>
public class PlayerSnapshot
{
    private readonly List<CardSnapshot> handCards;
    private readonly int handCount;

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
    /// 当前已知手牌快照。
    /// 如果模拟中发生了无法确定具体是哪张牌的变化，这个列表可能只代表已知部分。
    /// </summary>
    public IReadOnlyList<CardSnapshot> HandCards => handCards;

    /// <summary>
    /// 当前手牌数量。
    /// 数量可能大于 HandCards.Count，因为抽牌时当前快照还不知道牌库顶是哪张。
    /// </summary>
    public int HandCount => handCount;

    /// <summary>
    /// 当前牌库数量。
    /// </summary>
    public int DeckCount { get; private set; }

    public PlayerSnapshot(
        int heroHealth,
        int heroMaxHealth,
        int currentMana,
        int maxMana,
        int handCount,
        int deckCount,
        IReadOnlyList<CardSnapshot> handCards = null)
    {
        this.handCards = CopyHandCards(handCards);

        HeroMaxHealth = ClampNonNegative(heroMaxHealth);
        HeroHealth = Clamp(heroHealth, 0, HeroMaxHealth);
        MaxMana = ClampNonNegative(maxMana);
        CurrentMana = Clamp(currentMana, 0, MaxMana);
        this.handCount = ClampNonNegative(handCount);
        if (this.handCount < this.handCards.Count)
        {
            this.handCount = this.handCards.Count;
        }

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
        List<CardSnapshot> handCardSnapshots = CreateHandCardSnapshots(player.Hand);
        int handCount = player.Hand != null ? player.Hand.Count : handCardSnapshots.Count;
        int deckCount = player.Deck != null ? player.Deck.Count : 0;

        return new PlayerSnapshot(
            heroHealth,
            heroMaxHealth,
            player.CurrentMana,
            player.MaxMana,
            handCount,
            deckCount,
            handCardSnapshots);
    }

    private static List<CardSnapshot> CreateHandCardSnapshots(IReadOnlyList<Card> hand)
    {
        List<CardSnapshot> snapshots = new List<CardSnapshot>();
        if (hand == null) return snapshots;

        for (int i = 0; i < hand.Count; i++)
        {
            CardSnapshot snapshot = CardSnapshot.FromCard(hand[i]);
            if (snapshot == null) continue;

            snapshots.Add(snapshot);
        }

        return snapshots;
    }

    private static List<CardSnapshot> CopyHandCards(IReadOnlyList<CardSnapshot> source)
    {
        List<CardSnapshot> copy = new List<CardSnapshot>();
        if (source == null) return copy;

        for (int i = 0; i < source.Count; i++)
        {
            CardSnapshot card = source[i];
            if (card == null) continue;

            copy.Add(card);
        }

        return copy;
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
