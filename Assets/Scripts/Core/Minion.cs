/// <summary>
/// 随从 - 已经被召唤到战场上的运行时单位。
/// 它从 CardData 读取初始属性，但上场后的血量、攻击状态由自己维护。
/// </summary>
public class Minion
{
    public CardData CardData { get; private set; }
    public Player Owner { get; private set; }
    public int Attack { get; private set; }
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public bool CanAttack { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    public Minion(CardData cardData, Player owner)
    {
        CardData = cardData;
        Owner = owner;
        Attack = cardData.Attack;
        MaxHealth = cardData.Health;
        CurrentHealth = MaxHealth;
        CanAttack = false;
    }

    /// <summary>
    /// 受到伤害。
    /// </summary>
    /// <returns>实际造成的伤害值。</returns>
    public int TakeDamage(int amount)
    {
        if (amount <= 0) return 0;

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

    public override string ToString()
    {
        return $"{CardData.CardName} {Attack}/{CurrentHealth}";
    }
}
