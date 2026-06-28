/// <summary>
/// AI 模拟用的随从状态快照。
/// 它只保存模拟攻击、目标限制和局面评估需要的轻量数据。
/// </summary>
public class MinionSnapshot
{
    /// <summary>
    /// 当前攻击力。
    /// </summary>
    public int Attack { get; private set; }

    /// <summary>
    /// 当前血量。
    /// </summary>
    public int CurrentHealth { get; private set; }

    /// <summary>
    /// 最大血量。
    /// </summary>
    public int MaxHealth { get; private set; }

    /// <summary>
    /// 当前是否可以攻击。
    /// </summary>
    public bool CanAttack { get; private set; }

    /// <summary>
    /// 是否拥有嘲讽。
    /// </summary>
    public bool HasTaunt { get; private set; }

    /// <summary>
    /// 是否拥有圣盾。
    /// </summary>
    public bool HasDivineShield { get; private set; }

    /// <summary>
    /// 是否拥有冲锋。
    /// </summary>
    public bool HasCharge { get; private set; }

    /// <summary>
    /// 当前是否已经死亡。
    /// </summary>
    public bool IsDead => CurrentHealth <= 0;

    public MinionSnapshot(
        int attack,
        int currentHealth,
        int maxHealth,
        bool canAttack,
        bool hasTaunt,
        bool hasDivineShield,
        bool hasCharge)
    {
        Attack = ClampNonNegative(attack);
        MaxHealth = ClampNonNegative(maxHealth);
        CurrentHealth = Clamp(currentHealth, 0, MaxHealth);
        CanAttack = canAttack;
        HasTaunt = hasTaunt;
        HasDivineShield = hasDivineShield;
        HasCharge = hasCharge;
    }

    /// <summary>
    /// 从真实 Minion 复制一份轻量快照。
    /// </summary>
    public static MinionSnapshot FromMinion(Minion minion)
    {
        if (minion == null)
        {
            return null;
        }

        return new MinionSnapshot(
            minion.Attack,
            minion.CurrentHealth,
            minion.MaxHealth,
            minion.CanAttack,
            minion.HasKeyword(KeywordType.Taunt),
            minion.HasKeyword(KeywordType.DivineShield),
            minion.HasKeyword(KeywordType.Charge));
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
