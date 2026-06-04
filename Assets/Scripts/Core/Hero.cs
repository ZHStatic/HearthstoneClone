/// <summary>
/// 英雄 — 你的"主基地"
/// 血量归零即输掉游戏
/// </summary>
public class Hero
{
    // 英雄名字（例如"吉安娜""古尔丹"）
    public string Name { get; private set; }

    // 最大血量（炉石标准 30）
    public int MaxHealth { get; private set; }

    // 当前血量
    public int CurrentHealth { get; private set; }

    // 是否还活着
    public bool IsDead => CurrentHealth <= 0;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="name">英雄名称</param>
    /// <param name="maxHealth">初始/最大血量，默认 30</param>
    public Hero(string name, int maxHealth = 30)
    {
        Name = name;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }

    public override string ToString()
    {
        return $"{Name} ({CurrentHealth}/{MaxHealth})";
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="amount">伤害值</param>
    /// <returns>伤害值（原样返回）</returns>
    public int TakeDamage(int amount)
    {
        if (amount <= 0) return 0;

        CurrentHealth -= amount;
        return amount;
    }

    /// <summary>
    /// 恢复生命值（不能超过最大值）
    /// </summary>
    /// <param name="amount">恢复量</param>
    /// <returns>实际恢复的量</returns>
    public int Heal(int amount)
    {
        if (amount <= 0) return 0;

        int maxHeal = MaxHealth - CurrentHealth;
        int actualHeal = amount < maxHeal ? amount : maxHeal;

        CurrentHealth += actualHeal;
        return actualHeal;
    }
}
