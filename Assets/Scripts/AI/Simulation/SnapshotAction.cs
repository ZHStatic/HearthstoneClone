/// <summary>
/// AI 快照模拟用的动作数据。
/// 它只保存索引和数值，不引用真实 Card、Minion 或 Hero。
/// </summary>
public class SnapshotAction
{
    /// <summary>
    /// 动作类型，复用真实对局里的 GameActionType。
    /// </summary>
    public GameActionType ActionType { get; private set; }

    /// <summary>
    /// 动作发起方索引。
    /// 0 表示 Player，1 表示 Enemy。
    /// </summary>
    public int ActorIndex { get; private set; }

    /// <summary>
    /// 出牌或施法消耗的费用。
    /// </summary>
    public int CardCost { get; private set; }

    /// <summary>
    /// 召唤随从时使用的攻击力。
    /// </summary>
    public int CardAttack { get; private set; }

    /// <summary>
    /// 召唤随从时使用的生命值。
    /// </summary>
    public int CardHealth { get; private set; }

    /// <summary>
    /// 法术造成的伤害。
    /// </summary>
    public int SpellDamage { get; private set; }

    /// <summary>
    /// 打出的随从牌是否带有嘲讽。
    /// </summary>
    public bool CardHasTaunt { get; private set; }

    /// <summary>
    /// 打出的随从牌是否带有圣盾。
    /// </summary>
    public bool CardHasDivineShield { get; private set; }

    /// <summary>
    /// 打出的随从牌是否带有冲锋。
    /// </summary>
    public bool CardHasCharge { get; private set; }

    /// <summary>
    /// 打出的随从牌拥有的战吼类型。
    /// </summary>
    public BattlecryType BattlecryType { get; private set; }

    /// <summary>
    /// 战吼通用数值。
    /// </summary>
    public int BattlecryValue { get; private set; }

    /// <summary>
    /// 打出的随从牌拥有的亡语类型。
    /// </summary>
    public DeathrattleType DeathrattleType { get; private set; }

    /// <summary>
    /// 亡语通用数值。
    /// </summary>
    public int DeathrattleValue { get; private set; }

    /// <summary>
    /// 攻击动作中，攻击随从在己方场面列表里的索引。
    /// 非攻击动作使用 -1 表示没有攻击者。
    /// </summary>
    public int AttackerIndex { get; private set; }

    /// <summary>
    /// 目标随从在对手场面列表里的索引。
    /// 目标不是随从时使用 -1。
    /// </summary>
    public int TargetMinionIndex { get; private set; }

    /// <summary>
    /// 目标是否是英雄。
    /// </summary>
    public bool TargetsHero { get; private set; }

    private SnapshotAction(
        GameActionType actionType,
        int actorIndex,
        int cardCost,
        int cardAttack,
        int cardHealth,
        int spellDamage,
        bool cardHasTaunt,
        bool cardHasDivineShield,
        bool cardHasCharge,
        BattlecryType battlecryType,
        int battlecryValue,
        DeathrattleType deathrattleType,
        int deathrattleValue,
        int attackerIndex,
        int targetMinionIndex,
        bool targetsHero)
    {
        // 这里不做动作合法性判断，只把传入数据标准化成模拟器容易读取的形式。
        ActionType = actionType;
        ActorIndex = NormalizePlayerIndex(actorIndex);
        CardCost = ClampNonNegative(cardCost);
        CardAttack = ClampNonNegative(cardAttack);
        CardHealth = ClampNonNegative(cardHealth);
        SpellDamage = ClampNonNegative(spellDamage);
        CardHasTaunt = cardHasTaunt;
        CardHasDivineShield = cardHasDivineShield;
        CardHasCharge = cardHasCharge;
        BattlecryType = battlecryType;
        BattlecryValue = ClampNonNegative(battlecryValue);
        DeathrattleType = deathrattleType;
        DeathrattleValue = ClampNonNegative(deathrattleValue);
        AttackerIndex = attackerIndex;
        TargetMinionIndex = targetMinionIndex;
        TargetsHero = targetsHero;
    }

    /// <summary>
    /// 创建“打出随从牌”的快照动作。
    /// </summary>
    public static SnapshotAction CreatePlayMinion(
        int actorIndex,
        int cardCost,
        int cardAttack,
        int cardHealth,
        bool cardHasTaunt,
        bool cardHasDivineShield,
        bool cardHasCharge,
        BattlecryType battlecryType,
        int battlecryValue,
        DeathrattleType deathrattleType,
        int deathrattleValue)
    {
        return new SnapshotAction(
            GameActionType.PlayMinionCard,
            actorIndex,
            cardCost,
            cardAttack,
            cardHealth,
            0,
            cardHasTaunt,
            cardHasDivineShield,
            cardHasCharge,
            battlecryType,
            battlecryValue,
            deathrattleType,
            deathrattleValue,
            -1,
            -1,
            false);
    }

    /// <summary>
    /// 创建“对随从释放法术”的快照动作。
    /// </summary>
    public static SnapshotAction CreateSpellOnMinion(int actorIndex, int cardCost, int spellDamage, int targetMinionIndex)
    {
        return new SnapshotAction(
            GameActionType.PlaySpellOnMinion,
            actorIndex,
            cardCost,
            0,
            0,
            spellDamage,
            false,
            false,
            false,
            BattlecryType.None,
            0,
            DeathrattleType.None,
            0,
            -1,
            targetMinionIndex,
            false);
    }

    /// <summary>
    /// 创建“对英雄释放法术”的快照动作。
    /// </summary>
    public static SnapshotAction CreateSpellOnHero(int actorIndex, int cardCost, int spellDamage)
    {
        return new SnapshotAction(
            GameActionType.PlaySpellOnHero,
            actorIndex,
            cardCost,
            0,
            0,
            spellDamage,
            false,
            false,
            false,
            BattlecryType.None,
            0,
            DeathrattleType.None,
            0,
            -1,
            -1,
            true);
    }

    /// <summary>
    /// 创建“随从攻击随从”的快照动作。
    /// </summary>
    public static SnapshotAction CreateAttackMinion(int actorIndex, int attackerIndex, int targetMinionIndex)
    {
        return new SnapshotAction(
            GameActionType.AttackMinion,
            actorIndex,
            0,
            0,
            0,
            0,
            false,
            false,
            false,
            BattlecryType.None,
            0,
            DeathrattleType.None,
            0,
            attackerIndex,
            targetMinionIndex,
            false);
    }

    /// <summary>
    /// 创建“随从攻击英雄”的快照动作。
    /// </summary>
    public static SnapshotAction CreateAttackHero(int actorIndex, int attackerIndex)
    {
        return new SnapshotAction(
            GameActionType.AttackHero,
            actorIndex,
            0,
            0,
            0,
            0,
            false,
            false,
            false,
            BattlecryType.None,
            0,
            DeathrattleType.None,
            0,
            attackerIndex,
            -1,
            true);
    }

    /// <summary>
    /// 创建“结束回合”的快照动作。
    /// </summary>
    public static SnapshotAction CreateEndTurn(int actorIndex)
    {
        return new SnapshotAction(
            GameActionType.EndTurn,
            actorIndex,
            0,
            0,
            0,
            0,
            false,
            false,
            false,
            BattlecryType.None,
            0,
            DeathrattleType.None,
            0,
            -1,
            -1,
            false);
    }

    private static int NormalizePlayerIndex(int playerIndex)
    {
        // 模拟层当前只认识 Player / Enemy 两边；非法值保守归到 Player。
        return playerIndex == GameStateSnapshot.EnemyIndex
            ? GameStateSnapshot.EnemyIndex
            : GameStateSnapshot.PlayerIndex;
    }

    private static int ClampNonNegative(int value)
    {
        return value < 0 ? 0 : value;
    }
}
