/// <summary>
/// AI 模拟用的手牌卡牌快照。
/// 它只保存模拟出牌需要的轻量数据，不引用真实 Card 或 CardData。
/// </summary>
public class CardSnapshot
{
    /// <summary>
    /// 卡牌类型：随从或法术。
    /// </summary>
    public CardType CardType { get; private set; }

    /// <summary>
    /// 当前费用。来自真实 Card 时会复制 CurrentCost。
    /// </summary>
    public int Cost { get; private set; }

    /// <summary>
    /// 随从牌的攻击力。
    /// </summary>
    public int Attack { get; private set; }

    /// <summary>
    /// 随从牌的生命值。
    /// </summary>
    public int Health { get; private set; }

    /// <summary>
    /// 法术造成的伤害。
    /// </summary>
    public int SpellDamage { get; private set; }

    /// <summary>
    /// 法术可选择的目标范围。
    /// </summary>
    public SpellTargetType SpellTargetType { get; private set; }

    /// <summary>
    /// 这张随从牌是否带有嘲讽。
    /// </summary>
    public bool HasTaunt { get; private set; }

    /// <summary>
    /// 这张随从牌是否带有圣盾。
    /// </summary>
    public bool HasDivineShield { get; private set; }

    /// <summary>
    /// 这张随从牌是否带有冲锋。
    /// </summary>
    public bool HasCharge { get; private set; }

    /// <summary>
    /// 随从牌的战吼类型。
    /// </summary>
    public BattlecryType BattlecryType { get; private set; }

    /// <summary>
    /// 战吼通用数值。
    /// </summary>
    public int BattlecryValue { get; private set; }

    /// <summary>
    /// 随从牌的亡语类型。
    /// </summary>
    public DeathrattleType DeathrattleType { get; private set; }

    /// <summary>
    /// 亡语通用数值。
    /// </summary>
    public int DeathrattleValue { get; private set; }

    public CardSnapshot(
        CardType cardType,
        int cost,
        int attack,
        int health,
        int spellDamage,
        SpellTargetType spellTargetType,
        bool hasTaunt,
        bool hasDivineShield,
        bool hasCharge,
        BattlecryType battlecryType,
        int battlecryValue,
        DeathrattleType deathrattleType,
        int deathrattleValue)
    {
        CardType = cardType;
        Cost = ClampNonNegative(cost);
        Attack = ClampNonNegative(attack);
        Health = ClampNonNegative(health);
        SpellDamage = ClampNonNegative(spellDamage);
        SpellTargetType = spellTargetType;
        HasTaunt = hasTaunt;
        HasDivineShield = hasDivineShield;
        HasCharge = hasCharge;
        BattlecryType = battlecryType;
        BattlecryValue = ClampNonNegative(battlecryValue);
        DeathrattleType = deathrattleType;
        DeathrattleValue = ClampNonNegative(deathrattleValue);
    }

    /// <summary>
    /// 从真实手牌或牌库里的 Card 复制快照。
    /// </summary>
    public static CardSnapshot FromCard(Card card)
    {
        if (card == null || card.CardData == null)
        {
            return null;
        }

        return FromCardData(card.CardData, card.CurrentCost);
    }

    /// <summary>
    /// 从 CardData 模板复制快照，费用使用模板默认费用。
    /// </summary>
    public static CardSnapshot FromCardData(CardData cardData)
    {
        if (cardData == null)
        {
            return null;
        }

        return FromCardData(cardData, cardData.Cost);
    }

    private static CardSnapshot FromCardData(CardData cardData, int cost)
    {
        return new CardSnapshot(
            cardData.CardType,
            cost,
            cardData.Attack,
            cardData.Health,
            cardData.SpellDamage,
            cardData.SpellTargetType,
            cardData.HasKeyword(KeywordType.Taunt),
            cardData.HasKeyword(KeywordType.DivineShield),
            cardData.HasKeyword(KeywordType.Charge),
            cardData.BattlecryType,
            cardData.BattlecryValue,
            cardData.DeathrattleType,
            cardData.DeathrattleValue);
    }

    private static int ClampNonNegative(int value)
    {
        return value < 0 ? 0 : value;
    }
}
