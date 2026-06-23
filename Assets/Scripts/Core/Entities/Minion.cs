using System.Collections.Generic;

/// <summary>
/// 随从 - 已经被召唤到战场上的运行时单位。
/// 它从 CardData 读取初始属性，但上场后的血量、攻击状态由自己维护。
/// </summary>
public class Minion
{
    // 随从自己的关键词列表。
    // 不直接一直读取 CardData，是为了以后支持沉默、获得关键词、失去关键词等运行时变化。
    private readonly List<KeywordType> keywords = new List<KeywordType>();

    public CardData CardData { get; private set; }
    public Player Owner { get; private set; }
    public int Attack { get; private set; }
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public bool CanAttack { get; private set; }
    public bool IsDead => CurrentHealth <= 0;
    public bool HasDivineShield => HasKeyword(KeywordType.DivineShield);

    // 外部可以查看随从当前有哪些关键词，但不能替换关键词列表。
    public IReadOnlyList<KeywordType> Keywords => keywords;

    public Minion(CardData cardData, Player owner)
    {
        CardData = cardData;
        Owner = owner;
        Attack = cardData.Attack;
        MaxHealth = cardData.Health;
        CurrentHealth = MaxHealth;
        CanAttack = false;

        CopyKeywordsFromCardData(cardData);
    }

    /// <summary>
    /// 查询这个场上随从当前是否拥有指定关键词。
    /// </summary>
    public bool HasKeyword(KeywordType keyword)
    {
        if (keyword == KeywordType.None) return false;

        return keywords.Contains(keyword);
    }

    /// <summary>
    /// 受到伤害。
    /// </summary>
    /// <returns>实际造成的伤害值。</returns>
    public int TakeDamage(int amount)
    {
        if (amount <= 0) return 0;

        if (HasDivineShield)
        {
            RemoveKeyword(KeywordType.DivineShield);
            return 0;
        }

        CurrentHealth -= amount;
        return amount;
    }

    /// <summary>
    /// 恢复生命值，不能超过最大生命值。
    /// </summary>
    /// <returns>实际恢复的生命值。</returns>
    public int Heal(int amount)
    {
        if (amount <= 0) return 0;

        int missingHealth = MaxHealth - CurrentHealth;
        int actualHeal = amount < missingHealth ? amount : missingHealth;

        CurrentHealth += actualHeal;
        return actualHeal;
    }

    /// <summary>
    /// 设置随从本回合是否可以攻击。
    /// </summary>
    public void SetCanAttack(bool canAttack)
    {
        CanAttack = canAttack;
    }

    /// <summary>
    /// 移除一个运行时关键词。
    /// 当前先用于圣盾被伤害消耗，之后也可以给沉默或失去关键词效果复用。
    /// </summary>
    public void RemoveKeyword(KeywordType keyword)
    {
        if (keyword == KeywordType.None) return;

        keywords.Remove(keyword);
    }

    public override string ToString()
    {
        return $"{CardData.CardName} {Attack}/{CurrentHealth}";
    }

    /// <summary>
    /// 随从被创建时，从卡牌模板复制初始关键词。
    /// </summary>
    private void CopyKeywordsFromCardData(CardData cardData)
    {
        if (cardData == null || cardData.Keywords == null) return;

        foreach (KeywordType keyword in cardData.Keywords)
        {
            if (keyword == KeywordType.None) continue;
            if (keywords.Contains(keyword)) continue;

            keywords.Add(keyword);
        }
    }
}
