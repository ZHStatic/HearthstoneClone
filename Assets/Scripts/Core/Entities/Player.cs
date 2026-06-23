using System.Collections.Generic;

/// <summary>
/// 玩家 — 管理手牌、牌库、法力水晶和英雄
/// 对战时有己方玩家和对方玩家各一个实例
/// </summary>
public class Player
{
    private readonly List<Card> hand;
    private readonly List<Card> deck;

    // ── 英雄 ──
    public Hero Hero { get; private set; }

    // ── 手牌 & 牌库 ──
    public IReadOnlyList<Card> Hand => hand;
    public IReadOnlyList<Card> Deck => deck;

    // 手牌是否满了
    public bool IsHandFull => hand.Count >= MaxHandSize;

    // 牌库是否空了
    public bool IsDeckEmpty => deck.Count == 0;

    // ── 法力水晶 ──
    public int MaxMana { get; private set; }     // 本回合最大水晶数
    public int CurrentMana { get; private set; }  // 本回合剩余可用水晶

    // ── 常量 ──
    public const int MaxHandSize = 10;   // 手牌上限
    public const int MaxManaCap = 10;    // 水晶上限

    // ── 构造函数 ──
    /// <param name="deckCards">初始牌库的卡牌模板列表</param>
    /// <param name="heroName">英雄名称（如"吉安娜"）</param>
    /// <param name="heroHealth">英雄血量，默认 30</param>
    public Player(List<CardData> deckCards, string heroName = "未命名英雄", int heroHealth = 30)
    {
        Hero = new Hero(heroName, heroHealth);
        hand = new List<Card>();

        // 从模板创建卡牌实例，放入牌库。
        // Inspector 里可能会误留空槽位，所以这里要跳过空数据，避免开局崩溃。
        deck = new List<Card>();
        if (deckCards != null)
        {
            foreach (CardData data in deckCards)
            {
                if (data == null) continue;

                deck.Add(new Card(data));
            }
        }

        // 牌库洗牌
        ShuffleDeck();

        MaxMana = 0;
        CurrentMana = 0;
    }

    // ── 回合流程 ──

    /// <summary>
    /// 回合开始：水晶 +1 → 补满 → 重置卡牌费用 → 抽一张
    /// </summary>
    public void StartTurn()
    {
        // 增加 1 点水晶上限（最多 10）
        if (MaxMana < MaxManaCap)
            MaxMana++;

        // 补满当前水晶
        CurrentMana = MaxMana;

        // 重置所有手牌的费用（消除上回合的临时减费效果）
        foreach (Card card in hand)
            card.ResetCost();

        // 抽一张牌
        DrawCard();
    }

    /// <summary>
    /// 从牌库顶摸一张牌
    /// </summary>
    /// <returns>抽到的卡牌；牌库空或手牌满则返回 null</returns>
    public Card DrawCard()
    {
        // 牌库空了
        if (IsDeckEmpty) return null;

        // 手牌满了——卡牌直接烧毁（炉石规则）
        if (IsHandFull)
        {
            deck.RemoveAt(deck.Count - 1);
            return null;
        }

        // 从牌库顶抽一张（List 末尾 = 牌库顶）
        int topIndex = deck.Count - 1;
        Card drawnCard = deck[topIndex];
        deck.RemoveAt(topIndex);
        hand.Add(drawnCard);

        return drawnCard;
    }

    // ── 手牌操作 ──

    /// <summary>
    /// 打出手牌中某张卡，消耗法力水晶
    /// </summary>
    /// <param name="card">要打出的卡</param>
    /// <returns>是否成功打出</returns>
    public bool PlayCard(Card card)
    {
        if (card == null) return false;
        if (!HasCardInHand(card)) return false;
        if (card.CurrentCost > CurrentMana) return false;

        CurrentMana -= card.CurrentCost;
        hand.Remove(card);
        return true;
    }

    /// <summary>
    /// 检查这张运行时卡牌是否属于当前玩家手牌。
    /// 外部用这个方法判断归属，而不是直接修改手牌列表。
    /// </summary>
    public bool HasCardInHand(Card card)
    {
        return card != null && hand.Contains(card);
    }

    // ── 牌库操作 ──

    /// <summary>
    /// 洗牌——随机打乱牌库顺序
    /// </summary>
    public void ShuffleDeck()
    {
        // Fisher-Yates 洗牌算法
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            Card temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }
    }
}
